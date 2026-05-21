using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityCodeGen;
using UnityEditor;
using UnityEngine;

namespace StaticMlp.Editor.ReplicationCodeGen {
    [Generator]
    public sealed class ReplicationCodeGenerator : ICodeGenerator {
        private const string SharedOutputFolder = "Assets/Scripts/StaticMlp/Generated/ReplicationShared";
        private const string CharacterNetStateFullName = "StaticMlp.Game.Components.CharacterNetState";

        public void Execute(GeneratorContext context) {
            Directory.CreateDirectory(SharedOutputFolder);
            DeleteStaleSharedComponentOutputs();
            context.OverrideFolderPath(SharedOutputFolder);

            var diagnostics = new List<string>();
            var components = TypeCache.GetTypesWithAttribute<ReplicatedComponentAttribute>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .Select(t => CreateComponentInfo(t, diagnostics))
                .Where(x => x != null)
                .ToList();

            ValidateDuplicateIds(components, diagnostics);
            var networkEntities = CreateNetworkEntityInfos(components, diagnostics);
            ValidateDuplicateNetworkEntityIds(networkEntities, diagnostics);

            context.AddCode("StaticMlp.Replication.CodeGenDiagnostics.Generated.cs", EmitDiagnostics(diagnostics));
            if (diagnostics.Count != 0)
                return;

            context.OverrideFolderPath(SharedOutputFolder);
            context.AddCode("ReplicatedComponentIds.Generated.cs", EmitIds(components));
            foreach (var component in components) {
                Directory.CreateDirectory(component.GeneratedOutputFolder);
                WriteGeneratedCode(component.GeneratedOutputFolder, $"{component.Type.Name}.Replication.Generated.cs", EmitComponentReplication(component));
                if (component.NeedsPartialSerialization) {
                    WriteGeneratedCode(component.GeneratedOutputFolder, $"{component.Type.Name}.Replication.Partial.Generated.cs", EmitComponentPartial(component));
                } else {
                    DeleteGeneratedCode(component.GeneratedOutputFolder, $"{component.Type.Name}.Replication.Partial.Generated.cs");
                }
            }
            context.OverrideFolderPath(SharedOutputFolder);
            context.AddCode("ReplicatedComponentRegistration.Generated.cs", EmitRegistry(components, networkEntities));
        }

        private static void WriteGeneratedCode(string folder, string fileName, string code) {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, fileName), code, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void DeleteGeneratedCode(string folder, string fileName) {
            var path = Path.Combine(folder, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void DeleteStaleSharedComponentOutputs() {
            if (!Directory.Exists(SharedOutputFolder))
                return;

            foreach (var path in Directory.GetFiles(SharedOutputFolder, "*.Replication.Generated.cs"))
                File.Delete(path);
            foreach (var path in Directory.GetFiles(SharedOutputFolder, "*.Replication.Partial.Generated.cs"))
                File.Delete(path);
        }

        [MenuItem("StaticMlp/Replication/Generate")]
        private static void GenerateFromMenu() {
            EditorApplication.ExecuteMenuItem("Tools/UnityCodeGen/Generate");
        }

        private static ComponentInfo CreateComponentInfo(Type type, List<string> diagnostics) {
            var attribute = type.GetCustomAttribute<ReplicatedComponentAttribute>();
            if (attribute == null)
                return null;

            if (!type.IsValueType || type.IsEnum) {
                diagnostics.Add($"{type.FullName}: replicated component must be a struct.");
                return null;
            }

            if (!typeof(IComponent).IsAssignableFrom(type))
                diagnostics.Add($"{type.FullName}: replicated component must implement IComponent.");

            if (!typeof(ITrackableChanged).IsAssignableFrom(type))
                diagnostics.Add($"{type.FullName}: replicated component must implement ITrackableChanged so dirty deltas can be collected.");

            var attributeGuid = TryReadAttributeGuid(type, attribute, diagnostics);
            var configGuid = TryReadStaticEcsGuid(type);
            if (attributeGuid.HasValue && configGuid.HasValue && attributeGuid.Value != configGuid.Value)
                diagnostics.Add($"{type.FullName}: ReplicatedComponentAttribute guid does not match IComponentConfig<T>.Config() guid.");

            var guid = attributeGuid ?? configGuid;
            if (!guid.HasValue)
                diagnostics.Add($"{type.FullName}: replicated component must declare a stable guid in ReplicatedComponentAttribute.");

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(f => FieldInfoFor(f, diagnostics))
                .Where(x => x != null)
                .ToList();

            var sourcePath = FindSourcePath(type);
            var hasConfig = SourceDeclaresMethod(sourcePath, "Config");
            var hasWrite = SourceDeclaresMethod(sourcePath, "Write");
            var hasRead = SourceDeclaresMethod(sourcePath, "Read");
            if (fields.Count == 0 && (!hasWrite || !hasRead))
                diagnostics.Add($"{type.FullName}: replicated component has no public replicated fields and must declare both Write and Read.");
            if (sourcePath == null)
                diagnostics.Add($"{type.FullName}: source script path not found; cannot emit same-assembly generated replication.");
            var generatedOutputFolder = sourcePath == null ? null : FindSameAssemblyGeneratedFolder(sourcePath);
            if (generatedOutputFolder == null)
                diagnostics.Add($"{type.FullName}: nearest asmdef folder not found; cannot emit same-assembly generated replication.");

            return new ComponentInfo(
                type,
                fields,
                GetComponentId(type, guid),
                attribute.Authority,
                attribute.Audience,
                attribute.Delivery,
                attribute.SendRate,
                guid,
                generatedOutputFolder,
                hasConfig,
                hasWrite,
                hasRead
            );
        }

        private static bool SourceDeclaresMethod(string sourcePath, string methodName) {
            if (sourcePath == null)
                return false;

            var text = File.ReadAllText(sourcePath);
            return text.IndexOf($" {methodName}<", StringComparison.Ordinal) >= 0
                   || text.IndexOf($" {methodName}(", StringComparison.Ordinal) >= 0;
        }

        private static string FindSourcePath(Type type) {
            var guids = AssetDatabase.FindAssets($"{type.Name} t:MonoScript");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == type.Name)
                    return path;
            }

            return null;
        }

        private static string FindSameAssemblyGeneratedFolder(string sourcePath) {
            var folder = Path.GetDirectoryName(sourcePath);
            while (!string.IsNullOrEmpty(folder)) {
                if (Directory.GetFiles(folder, "*.asmdef").Length != 0)
                    return Path.Combine(folder, "Generated").Replace('\\', '/');

                var parent = Path.GetDirectoryName(folder);
                if (parent == folder)
                    break;

                folder = parent;
            }

            return null;
        }

        private static FieldReplicationInfo FieldInfoFor(FieldInfo field, List<string> diagnostics) {
            var attribute = field.GetCustomAttribute<ReplicatedFieldAttribute>();
            if (attribute == null)
                return null;

            var kind = FieldKind.From(field.FieldType);
            if (!kind.IsSupported) {
                diagnostics.Add($"{field.DeclaringType.FullName}.{field.Name}: unsupported replicated field type {field.FieldType.FullName}.");
                return null;
            }

            if (attribute.AllowZeroEntityGid && kind.Name != "EntityGID")
                diagnostics.Add($"{field.DeclaringType.FullName}.{field.Name}: AllowZeroEntityGid is supported only for EntityGID fields.");

            if (attribute.Quantize > 0f && !kind.CanQuantize)
                diagnostics.Add($"{field.DeclaringType.FullName}.{field.Name}: Quantize is supported only for float, Vector2, Vector3 and Quaternion.");

            if (attribute.Interpolation != ReplicatedFieldInterpolation.None && !kind.CanInterpolate)
                diagnostics.Add($"{field.DeclaringType.FullName}.{field.Name}: Interpolation.Auto is supported only for float, Vector2, Vector3 and Quaternion.");

            return new FieldReplicationInfo(field, attribute, kind);
        }

        private static Guid? TryReadAttributeGuid(Type type, ReplicatedComponentAttribute attribute, List<string> diagnostics) {
            if (string.IsNullOrWhiteSpace(attribute.Guid))
                return null;

            if (Guid.TryParse(attribute.Guid, out var guid))
                return guid;

            diagnostics.Add($"{type.FullName}: ReplicatedComponentAttribute guid `{attribute.Guid}` is not a valid GUID.");
            return null;
        }

        private static Guid? TryReadStaticEcsGuid(Type type) {
            try {
                var configMethod = type.GetMethod("Config", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                if (configMethod == null)
                    return null;

                var component = Activator.CreateInstance(type);
                var config = configMethod.Invoke(component, Array.Empty<object>());
                var guidField = config?.GetType().GetField("Guid", BindingFlags.Instance | BindingFlags.Public);
                return guidField?.GetValue(config) as Guid?;
            } catch {
                return null;
            }
        }

        private static ushort GetComponentId(Type type, Guid? guid) {
            if (type.FullName == CharacterNetStateFullName)
                return 1;

            var source = guid?.ToString("N") ?? type.FullName;
            var hash = 2166136261u;
            foreach (var ch in source) {
                hash ^= ch;
                hash *= 16777619u;
            }

            return (ushort)(100 + hash % 60000);
        }

        private static void ValidateDuplicateIds(List<ComponentInfo> components, List<string> diagnostics) {
            foreach (var group in components.GroupBy(x => x.TypeId).Where(g => g.Count() > 1)) {
                var names = string.Join(", ", group.Select(x => x.Type.FullName));
                diagnostics.Add($"Duplicate replicated component type id {group.Key}: {names}.");
            }
        }

        private static List<NetworkEntityInfo> CreateNetworkEntityInfos(List<ComponentInfo> components, List<string> diagnostics) {
            var byType = components.ToDictionary(x => x.Type);
            var result = new List<NetworkEntityInfo>();

            foreach (var type in TypeCache.GetTypesDerivedFrom<INetworkEntityType>()
                         .Where(t => t.IsValueType && !t.IsAbstract && !t.IsGenericTypeDefinition)
                         .OrderBy(t => t.FullName, StringComparer.Ordinal)) {
                if (Activator.CreateInstance(type) is not INetworkEntityType entityType) {
                    diagnostics.Add($"{type.FullName}: network entity type could not be instantiated.");
                    continue;
                }

                var manifestAttribute = type.GetCustomAttribute<NetworkEntityManifestAttribute>();
                if (manifestAttribute == null) {
                    diagnostics.Add($"{type.FullName}: network entity type must declare NetworkEntityManifestAttribute.");
                    continue;
                }

                var manifest = new List<ComponentInfo>();
                foreach (var componentType in manifestAttribute.ReplicatedComponents) {
                    if (componentType == typeof(NetworkIdentity))
                        continue;

                    if (!byType.TryGetValue(componentType, out var component)) {
                        diagnostics.Add($"{type.FullName}: manifest component {componentType.FullName} is not a generated replicated component.");
                        continue;
                    }

                    manifest.Add(component);
                }

                result.Add(new NetworkEntityInfo(
                    type,
                    entityType.Id(),
                    entityType.NetworkSchemaVersion(),
                    entityType.DefaultNetworkArchetypeId(),
                    manifest));
            }

            return result;
        }

        private static void ValidateDuplicateNetworkEntityIds(List<NetworkEntityInfo> entities, List<string> diagnostics) {
            foreach (var group in entities.GroupBy(x => x.EntityTypeId).Where(g => g.Count() > 1)) {
                var names = string.Join(", ", group.Select(x => x.Type.FullName));
                diagnostics.Add($"Duplicate network entity type id {group.Key}: {names}.");
            }
        }

        private static string EmitDiagnostics(List<string> diagnostics) {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine("    public static class ReplicationCodeGenDiagnostics { }");
            builder.AppendLine("}");
            foreach (var diagnostic in diagnostics)
                builder.AppendLine($"#error {EscapeError(diagnostic)}");
            return builder.ToString();
        }

        private static string EmitIds(List<ComponentInfo> components) {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("namespace StaticMlp.Networking.Replication {");
            builder.AppendLine("    public static class ReplicatedComponentIds {");
            foreach (var component in components)
                builder.AppendLine($"        public const ushort {component.Type.Name} = {component.TypeId};");
            builder.AppendLine("        public const ushort NetworkIdentity = 2;");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string EmitComponentReplication(ComponentInfo component) {
            var builder = new StringBuilder();
            var typeName = GetTypeName(component.Type);
            var replicationName = $"{component.Type.Name}Replication";

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("using System;");
            builder.AppendLine("using FFS.Libraries.StaticEcs;");
            builder.AppendLine($"using {component.Type.Namespace};");
            foreach (var ns in GetRequiredFieldNamespaces(component))
                builder.AppendLine($"using {ns};");
            builder.AppendLine("using StaticMlp.Networking;");
            if (component.Fields.Any(x => x.Kind.NeedsUnityEngine) || component.HasInterpolatedFields)
                builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine($"    public static class {replicationName} {{");
            builder.AppendLine($"        public const ushort TypeId = {component.TypeId};");
            builder.AppendLine($"        public const ReplicationAuthority Authority = ReplicationAuthority.{component.Authority};");
            builder.AppendLine($"        public const ReplicationAudience Audience = ReplicationAudience.{component.Audience};");
            builder.AppendLine($"        public const NetDelivery Delivery = NetDelivery.{component.Delivery};");
            builder.AppendLine($"        public const ushort SendRate = {component.SendRate};");
            builder.AppendLine("        public const byte LayoutVersion = 1;");
            if (component.HasInterpolatedFields) {
                builder.AppendLine($"        public static void Interpolate(in {typeName} previous, in {typeName} current, ref {typeName} interpolated, float alpha) {{");
                foreach (var field in component.Fields)
                    AppendInterpolate(builder, field);
                builder.AppendLine("        }");
            }
            if (component.Fields.Any(x => x.Attribute.Quantize > 0f)) {
                foreach (var quantize in component.Fields.Select(x => x.Attribute.Quantize).Where(x => x > 0f).Distinct())
                    AppendQuantize(builder, quantize);
            }
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string EmitComponentPartial(ComponentInfo component) {
            var builder = new StringBuilder();
            var typeName = GetTypeName(component.Type);
            var interfaceSuffix = component.HasConfig ? string.Empty : $" : IComponentConfig<{typeName}>";

            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("using System;");
            builder.AppendLine("using FFS.Libraries.StaticEcs;");
            builder.AppendLine("using FFS.Libraries.StaticPack;");
            foreach (var ns in GetRequiredFieldNamespaces(component))
                builder.AppendLine($"using {ns};");
            if (component.Fields.Any(x => x.Kind.NeedsUnityEngine))
                builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine($"namespace {component.Type.Namespace} {{");
            builder.AppendLine($"    public partial struct {typeName}{interfaceSuffix} {{");

            if (!component.HasConfig) {
                builder.AppendLine($"        public ComponentTypeConfig<{typeName}> Config() =>");
                builder.AppendLine($"            new(guid: new Guid(\"{component.Guid.Value}\"));");
                builder.AppendLine();
            }

            if (!component.HasWrite) {
                builder.AppendLine("        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)");
                builder.AppendLine("            where TWorld : struct, IWorldType {");
                foreach (var field in component.Fields)
                    AppendWriteHook(builder, field);
                builder.AppendLine("        }");
                if (!component.HasRead)
                    builder.AppendLine();
            }

            if (!component.HasRead) {
                builder.AppendLine("        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)");
                builder.AppendLine("            where TWorld : struct, IWorldType {");
                foreach (var field in component.Fields)
                    AppendReadHook(builder, field);
                builder.AppendLine("        }");
                if (NeedsOptionalEntityGidHelper(component)) {
                    builder.AppendLine();
                    builder.AppendLine("        private static EntityGID ReadOptionalEntityGid(ref BinaryPackReader reader) {");
                    builder.AppendLine("            var raw = reader.ReadUlong();");
                    builder.AppendLine("            return raw == 0ul ? default : new EntityGID(raw);");
                    builder.AppendLine("        }");
                }
            }

            if (!component.HasWrite && component.Fields.Any(x => x.Attribute.Quantize > 0f)) {
                foreach (var quantize in component.Fields.Select(x => x.Attribute.Quantize).Where(x => x > 0f).Distinct())
                    AppendQuantize(builder, quantize);
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static IEnumerable<string> GetRequiredFieldNamespaces(ComponentInfo component) {
            return component.Fields
                .Select(field => field.Field.FieldType.Namespace)
                .Where(ns => !string.IsNullOrEmpty(ns) && ns != component.Type.Namespace && ns != "System" && ns != "UnityEngine")
                .Distinct()
                .OrderBy(ns => ns, StringComparer.Ordinal);
        }

        private static string EmitRegistry(List<ComponentInfo> components, List<NetworkEntityInfo> networkEntities) {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("using FFS.Libraries.StaticEcs;");
            builder.AppendLine("using StaticMlp.Game.Bootstrap;");
            builder.AppendLine("using StaticMlp.Networking;");
            builder.AppendLine("using StaticMlp.Networking.Replication;");
            if (components.Any(x => x.HasInterpolatedFields))
                builder.AppendLine("using UnityEngine;");
            foreach (var ns in components.Select(x => x.Type.Namespace).Distinct().OrderBy(x => x, StringComparer.Ordinal))
                builder.AppendLine($"using {ns};");
            builder.AppendLine();
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine("    public static class ReplicatedComponentRegistration {");
            builder.AppendLine("        public static void RegisterReplicationComponents() {");
            builder.AppendLine("            ReplicationRegistry.Clear();");
            foreach (var component in components) {
                var name = GetTypeName(component.Type);
                builder.AppendLine();
                builder.AppendLine($"            ReplicationRegistry.RegisterComponent<{name}>(");
                builder.AppendLine($"                ReplicatedComponentIds.{component.Type.Name},");
                builder.AppendLine($"                ReplicationAuthority.{component.Authority},");
                builder.AppendLine($"                ReplicationAudience.{component.Audience},");
                builder.AppendLine($"                NetDelivery.{component.Delivery},");
                builder.AppendLine($"                sendRate: {component.Type.Name}Replication.SendRate{RegistrationCallbacks(component)});");
            }
            builder.AppendLine();
            foreach (var entity in networkEntities)
                builder.AppendLine($"            ReplicationRegistry.RegisterNetworkEntity({entity.EntityTypeId}, {entity.NetworkSchemaVersion}, {entity.DefaultNetworkArchetypeId});");
            builder.AppendLine("        }");
            foreach (var component in components.Where(x => x.HasInterpolatedFields))
                AppendRegistrationInterpolationHelpers(builder, component);
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string RegistrationCallbacks(ComponentInfo component) {
            if (!component.HasInterpolatedFields)
                return string.Empty;

            var name = component.Type.Name;
            return $",\n                registerClientTypes: Register{name}ClientTypes,\n                registerClientSystems: Register{name}ClientSystems,\n                initializeClientState: Initialize{name}ClientState";
        }

        private static void AppendRegistrationInterpolationHelpers(StringBuilder builder, ComponentInfo component) {
            var typeName = GetTypeName(component.Type);
            var name = component.Type.Name;

            builder.AppendLine();
            builder.AppendLine($"        private static void Register{name}ClientTypes() {{");
            builder.AppendLine("            CW.Types()");
            builder.AppendLine($"                .Component<Interpolated<{typeName}>>()");
            builder.AppendLine($"                .Component<InterpolatedPrevious<{typeName}>>()");
            builder.AppendLine($"                .Component<InterpolatedClock<{typeName}>>();");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine($"        private static void Register{name}ClientSystems(ClientCoreSystemsBuilder systems) {{");
            builder.AppendLine($"            systems.Add(new ReplicatedInterpolationSystem<{typeName}>({name}Replication.Interpolate), GameplaySystemOrder.ClientPresentation);");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine($"        private static void Initialize{name}ClientState(CW.Entity e) {{");
            builder.AppendLine($"            if (e.Has<{typeName}>() && !e.Has<Interpolated<{typeName}>>()) {{");
            builder.AppendLine($"                var state = e.Read<{typeName}>();");
            builder.AppendLine($"                e.Set(new Interpolated<{typeName}>(state));");
            builder.AppendLine($"                e.Set(new InterpolatedPrevious<{typeName}>(state));");
            builder.AppendLine($"                e.Set(new InterpolatedClock<{typeName}> {{");
            builder.AppendLine("                    StartedAt = Time.time,");
            builder.AppendLine($"                    Duration = {name}Replication.SendRate == 0 ? 0f : 1f / {name}Replication.SendRate");
            builder.AppendLine("                });");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
        }

        private static void AppendInterpolate(StringBuilder builder, FieldReplicationInfo field) {
            var name = field.Field.Name;
            if (field.Attribute.Interpolation == ReplicatedFieldInterpolation.None) {
                builder.AppendLine($"            interpolated.{name} = current.{name};");
                return;
            }

            switch (field.Kind.Name) {
                case "Float":
                    builder.AppendLine($"            interpolated.{name} = previous.{name} + (current.{name} - previous.{name}) * alpha;");
                    break;
                case "Vector2":
                    builder.AppendLine($"            interpolated.{name} = Vector2.LerpUnclamped(previous.{name}, current.{name}, alpha);");
                    break;
                case "Vector3":
                    builder.AppendLine($"            interpolated.{name} = Vector3.LerpUnclamped(previous.{name}, current.{name}, alpha);");
                    break;
                case "Quaternion":
                    builder.AppendLine($"            interpolated.{name} = Quaternion.Slerp(previous.{name}, current.{name}, alpha);");
                    break;
                default:
                    builder.AppendLine($"            interpolated.{name} = current.{name};");
                    break;
            }
        }

        private static void AppendQuantize(StringBuilder builder, float value) {
            var suffix = QuantizeSuffix(value);
            var literal = FloatLiteral(value);
            builder.AppendLine();
            builder.AppendLine($"        private static float Quantize{suffix}(float value) {{");
            builder.AppendLine($"            return (float)Math.Round(value / {literal}) * {literal};");
            builder.AppendLine("        }");
        }


        private static string MaybeQuantize(string method, string expression) {
            return method == null ? expression : $"{method}({expression})";
        }

        private static string QuantizeMethod(float value) {
            return $"Quantize{QuantizeSuffix(value)}";
        }

        private static string QuantizeSuffix(float value) {
            var text = value.ToString("0.########", CultureInfo.InvariantCulture);
            var builder = new StringBuilder();
            foreach (var ch in text) {
                if (char.IsDigit(ch))
                    builder.Append(ch);
            }
            return builder.Length == 0 ? "Value" : builder.ToString();
        }

        private static string FloatLiteral(float value) {
            return value.ToString("0.########", CultureInfo.InvariantCulture) + "f";
        }

        private static bool NeedsOptionalEntityGidHelper(ComponentInfo component) {
            return component.Fields.Any(x => x.Kind.Name == "EntityGID" && x.Attribute.AllowZeroEntityGid);
        }

        private static string GetTypeName(Type type) {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(float)) return "float";
            if (type == typeof(EntityGID)) return "EntityGID";
            return type.Name;
        }

        private static string EscapeError(string text) {
            return text.Replace("\\", "/").Replace("\r", " ").Replace("\n", " ");
        }

        private sealed class ComponentInfo {
            public readonly Type Type;
            public readonly List<FieldReplicationInfo> Fields;
            public readonly ushort TypeId;
            public readonly ReplicationAuthority Authority;
            public readonly ReplicationAudience Audience;
            public readonly NetDelivery Delivery;
            public readonly ushort SendRate;
            public readonly Guid? Guid;
            public readonly string GeneratedOutputFolder;
            public readonly bool HasConfig;
            public readonly bool HasWrite;
            public readonly bool HasRead;
            public bool HasInterpolatedFields => Fields.Any(x => x.Attribute.Interpolation != ReplicatedFieldInterpolation.None);
            public bool NeedsPartialSerialization => !HasConfig || !HasWrite || !HasRead;

            public ComponentInfo(
                Type type,
                List<FieldReplicationInfo> fields,
                ushort typeId,
                ReplicationAuthority authority,
                ReplicationAudience audience,
                NetDelivery delivery,
                ushort sendRate,
                Guid? guid,
                string generatedOutputFolder,
                bool hasConfig,
                bool hasWrite,
                bool hasRead) {
                Type = type;
                Fields = fields;
                TypeId = typeId;
                Authority = authority;
                Audience = audience;
                Delivery = delivery;
                SendRate = sendRate;
                Guid = guid;
                GeneratedOutputFolder = generatedOutputFolder;
                HasConfig = hasConfig;
                HasWrite = hasWrite;
                HasRead = hasRead;
            }
        }

        private static void AppendReadHook(StringBuilder builder, FieldReplicationInfo field) {
            var name = field.Field.Name;

            switch (field.Kind.Name) {
                case "Vector2":
                    builder.AppendLine($"            {name} = new Vector2(reader.ReadFloat(), reader.ReadFloat());");
                    break;
                case "Vector3":
                    builder.AppendLine($"            {name} = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());");
                    break;
                case "Quaternion":
                    builder.AppendLine($"            {name} = new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());");
                    break;
                case "EntityGID":
                    builder.AppendLine(field.Attribute.AllowZeroEntityGid
                        ? $"            {name} = ReadOptionalEntityGid(ref reader);"
                        : $"            {name} = new EntityGID(reader.ReadUlong());");
                    break;
                case "EnumByte":
                    builder.AppendLine($"            {name} = ({GetTypeName(field.Field.FieldType)})reader.ReadByte();");
                    break;
                case "EnumUshort":
                    builder.AppendLine($"            {name} = ({GetTypeName(field.Field.FieldType)})reader.ReadUshort();");
                    break;
                case "EnumInt":
                    builder.AppendLine($"            {name} = ({GetTypeName(field.Field.FieldType)})reader.ReadInt();");
                    break;
                default:
                    builder.AppendLine($"            {name} = reader.{field.Kind.ReadMethod}();");
                    break;
            }
        }

        private static void AppendWriteHook(StringBuilder builder, FieldReplicationInfo field) {
            var source = field.Field.Name;
            var q = field.Attribute.Quantize > 0f ? QuantizeMethod(field.Attribute.Quantize) : null;

            switch (field.Kind.Name) {
                case "Vector2":
                    builder.AppendLine($"            writer.WriteFloat({MaybeQuantize(q, $"{source}.x")}, {MaybeQuantize(q, $"{source}.y")});");
                    break;
                case "Vector3":
                    builder.AppendLine($"            writer.WriteFloat({MaybeQuantize(q, $"{source}.x")}, {MaybeQuantize(q, $"{source}.y")}, {MaybeQuantize(q, $"{source}.z")});");
                    break;
                case "Quaternion":
                    builder.AppendLine($"            writer.WriteFloat({MaybeQuantize(q, $"{source}.x")}, {MaybeQuantize(q, $"{source}.y")}, {MaybeQuantize(q, $"{source}.z")}, {MaybeQuantize(q, $"{source}.w")});");
                    break;
                case "EntityGID":
                    builder.AppendLine($"            writer.WriteUlong({source}.Raw);");
                    break;
                case "EnumByte":
                    builder.AppendLine($"            writer.WriteByte((byte){source});");
                    break;
                case "EnumUshort":
                    builder.AppendLine($"            writer.WriteUshort((ushort){source});");
                    break;
                case "EnumInt":
                    builder.AppendLine($"            writer.WriteInt((int){source});");
                    break;
                default:
                    builder.AppendLine($"            writer.{field.Kind.WriteMethod}({MaybeQuantize(q, source)});");
                    break;
            }
        }

        private sealed class NetworkEntityInfo {
            public readonly Type Type;
            public readonly byte EntityTypeId;
            public readonly ushort NetworkSchemaVersion;
            public readonly ushort DefaultNetworkArchetypeId;
            public readonly List<ComponentInfo> Manifest;

            public NetworkEntityInfo(
                Type type,
                byte entityTypeId,
                ushort networkSchemaVersion,
                ushort defaultNetworkArchetypeId,
                List<ComponentInfo> manifest) {
                Type = type;
                EntityTypeId = entityTypeId;
                NetworkSchemaVersion = networkSchemaVersion;
                DefaultNetworkArchetypeId = defaultNetworkArchetypeId;
                Manifest = manifest;
            }
        }

        private sealed class FieldReplicationInfo {
            public readonly FieldInfo Field;
            public readonly ReplicatedFieldAttribute Attribute;
            public readonly FieldKind Kind;

            public FieldReplicationInfo(FieldInfo field, ReplicatedFieldAttribute attribute, FieldKind kind) {
                Field = field;
                Attribute = attribute;
                Kind = kind;
            }
        }

        private readonly struct FieldKind {
            public readonly string Name;
            public readonly string WriteMethod;
            public readonly string ReadMethod;
            public readonly int ByteSize;
            public readonly bool CanQuantize;
            public readonly bool CanInterpolate;
            public readonly bool NeedsUnityEngine;
            public bool IsSupported => Name != null;

            private FieldKind(string name, string writeMethod, string readMethod, int byteSize, bool canQuantize = false, bool canInterpolate = false, bool needsUnityEngine = false) {
                Name = name;
                WriteMethod = writeMethod;
                ReadMethod = readMethod;
                ByteSize = byteSize;
                CanQuantize = canQuantize;
                CanInterpolate = canInterpolate;
                NeedsUnityEngine = needsUnityEngine;
            }

            public static FieldKind From(Type type) {
                if (type == typeof(bool)) return new FieldKind("Bool", "WriteBool", "ReadBool", 1);
                if (type == typeof(byte)) return new FieldKind("Byte", "WriteByte", "ReadByte", 1);
                if (type == typeof(sbyte)) return new FieldKind("Sbyte", "WriteSbyte", "ReadSByte", 1);
                if (type == typeof(short)) return new FieldKind("Short", "WriteShort", "ReadShort", 2);
                if (type == typeof(ushort)) return new FieldKind("Ushort", "WriteUshort", "ReadUshort", 2);
                if (type == typeof(int)) return new FieldKind("Int", "WriteInt", "ReadInt", 4);
                if (type == typeof(uint)) return new FieldKind("Uint", "WriteUint", "ReadUint", 4);
                if (type == typeof(long)) return new FieldKind("Long", "WriteLong", "ReadLong", 8);
                if (type == typeof(ulong)) return new FieldKind("Ulong", "WriteUlong", "ReadUlong", 8);
                if (type == typeof(float)) return new FieldKind("Float", "WriteFloat", "ReadFloat", 4, canQuantize: true, canInterpolate: true);
                if (type == typeof(EntityGID)) return new FieldKind("EntityGID", null, null, 8);
                if (type == typeof(Vector2)) return new FieldKind("Vector2", null, null, 8, canQuantize: true, canInterpolate: true, needsUnityEngine: true);
                if (type == typeof(Vector3)) return new FieldKind("Vector3", null, null, 12, canQuantize: true, canInterpolate: true, needsUnityEngine: true);
                if (type == typeof(Quaternion)) return new FieldKind("Quaternion", null, null, 16, canQuantize: true, canInterpolate: true, needsUnityEngine: true);
                if (type.IsEnum) {
                    var underlying = Enum.GetUnderlyingType(type);
                    if (underlying == typeof(byte)) return new FieldKind("EnumByte", null, null, 1);
                    if (underlying == typeof(ushort)) return new FieldKind("EnumUshort", null, null, 2);
                    if (underlying == typeof(int)) return new FieldKind("EnumInt", null, null, 4);
                }
                return default;
            }
        }
    }
}
