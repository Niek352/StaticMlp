using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string OutputFolder = "Assets/Scripts/StaticMlp/Game/ReplicationGenerated";
        private const string CharacterNetStateFullName = "StaticMlp.Game.Components.CharacterNetState";

        public void Execute(GeneratorContext context) {
            context.OverrideFolderPath(OutputFolder);

            var diagnostics = new List<string>();
            var components = TypeCache.GetTypesWithAttribute<ReplicatedComponentAttribute>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .Select(t => CreateComponentInfo(t, diagnostics))
                .Where(x => x != null)
                .ToList();

            ValidateDuplicateIds(components, diagnostics);

            context.AddCode("StaticMlp.Replication.CodeGenDiagnostics.Generated.cs", EmitDiagnostics(diagnostics));
            if (diagnostics.Count != 0)
                return;

            context.AddCode("ReplicatedComponentIds.Generated.cs", EmitIds(components));
            foreach (var component in components)
                context.AddCode($"{component.Type.Name}.Replication.Generated.cs", EmitComponentReplication(component));
            context.AddCode("ReplicatedComponentRegistry.Generated.cs", EmitRegistry(components));
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

            var guid = TryReadStaticEcsGuid(type);
            if (!guid.HasValue)
                diagnostics.Add($"{type.FullName}: replicated component must expose a stable StaticEcs guid through IComponentConfig<T>.Config().");

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(f => FieldInfoFor(f, diagnostics))
                .Where(x => x != null)
                .ToList();

            if (fields.Count == 0)
                diagnostics.Add($"{type.FullName}: replicated component has no public replicated fields.");

            return new ComponentInfo(
                type,
                fields,
                GetComponentId(type, guid),
                attribute.Authority,
                attribute.Delivery,
                attribute.SendRate
            );
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

            if (attribute.Quantize > 0f && !kind.CanQuantize)
                diagnostics.Add($"{field.DeclaringType.FullName}.{field.Name}: Quantize is supported only for float, Vector2, Vector3 and Quaternion.");

            return new FieldReplicationInfo(field, attribute, kind);
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
            builder.AppendLine("using FFS.Libraries.StaticPack;");
            builder.AppendLine($"using {component.Type.Namespace};");
            builder.AppendLine("using StaticMlp.Networking;");
            if (component.Fields.Any(x => x.Kind.NeedsUnityEngine))
                builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine($"    public static class {replicationName} {{");
            builder.AppendLine($"        public const ushort TypeId = ReplicatedComponentIds.{component.Type.Name};");
            builder.AppendLine($"        public const ReplicationAuthority Authority = ReplicationAuthority.{component.Authority};");
            builder.AppendLine($"        public const NetDelivery Delivery = NetDelivery.{component.Delivery};");
            builder.AppendLine($"        public const ushort SendRate = {component.SendRate};");
            builder.AppendLine("        public const byte LayoutVersion = 1;");
            builder.AppendLine();
            builder.AppendLine($"        public static ComponentDelta CreateDelta(EntityGID gid, in {typeName} state) {{");
            builder.AppendLine($"            var writer = BinaryPackWriter.CreateFromPool({EstimateBufferSize(component)});");
            foreach (var field in component.Fields)
                AppendWrite(builder, field);
            builder.AppendLine("            var bytes = writer.CopyToBytes();");
            builder.AppendLine("            writer.Dispose();");
            builder.AppendLine("            return new ComponentDelta(gid, TypeId, bytes);");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine($"        public static {typeName} Read(byte[] payload) {{");
            builder.AppendLine("            if (payload == null || payload.Length == 0)");
            builder.AppendLine("                return default;");
            builder.AppendLine();
            builder.AppendLine("            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);");
            builder.AppendLine($"            return new {typeName} {{");
            foreach (var field in component.Fields)
                AppendRead(builder, field);
            builder.AppendLine("            };");
            builder.AppendLine("        }");
            if (component.Fields.Any(x => x.Attribute.Quantize > 0f)) {
                foreach (var quantize in component.Fields.Select(x => x.Attribute.Quantize).Where(x => x > 0f).Distinct())
                    AppendQuantize(builder, quantize);
            }
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string EmitRegistry(List<ComponentInfo> components) {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using FFS.Libraries.StaticEcs;");
            builder.AppendLine("using StaticMlp.Networking;");
            builder.AppendLine("using StaticMlp.Networking.Ownership;");
            foreach (var ns in components.Select(x => x.Type.Namespace).Distinct().OrderBy(x => x, StringComparer.Ordinal))
                builder.AppendLine($"using {ns};");
            builder.AppendLine("using StaticMlp.Networking.Replication.Generated;");
            builder.AppendLine();
            builder.AppendLine("namespace StaticMlp.Networking.Replication {");
            builder.AppendLine("    public static partial class ReplicationRegistry {");
            AppendApply(builder, "CW", components);
            builder.AppendLine();
            AppendApply(builder, "SW", components);
            builder.AppendLine();
            AppendCollect(builder, "CW", components);
            builder.AppendLine();
            AppendCollect(builder, "SW", components);
            builder.AppendLine();
            AppendCollectClientOwnedDirty(builder, components);
            builder.AppendLine();
            AppendCollectServerOwnedDirty(builder, components);
            builder.AppendLine();
            AppendInitialState(builder, components);
            foreach (var component in components) {
                builder.AppendLine();
                builder.AppendLine($"        public static ComponentDelta CreateDelta(EntityGID gid, in {GetTypeName(component.Type)} state) {{");
                builder.AppendLine($"            return {component.Type.Name}Replication.CreateDelta(gid, state);");
                builder.AppendLine("        }");
            }
            builder.AppendLine();
            builder.AppendLine("        public static ComponentDelta CreateDelta(EntityGID gid, in NetworkIdentity identity) {");
            builder.AppendLine("            return NetworkIdentityReplication.CreateDelta(gid, identity);");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendApply(StringBuilder builder, string worldAlias, List<ComponentInfo> components) {
            builder.AppendLine($"        public static void ApplyDelta({worldAlias}.Entity e, ComponentDelta delta) {{");
            builder.AppendLine("            switch (delta.ComponentTypeId) {");
            foreach (var component in components) {
                builder.AppendLine($"                case ReplicatedComponentIds.{component.Type.Name}:");
                builder.AppendLine($"                    e.Set({component.Type.Name}Replication.Read(delta.Payload));");
                builder.AppendLine("                    break;");
            }
            builder.AppendLine("                case ReplicatedComponentIds.NetworkIdentity:");
            builder.AppendLine("                    e.Set(NetworkIdentityReplication.Read(delta.Payload));");
            builder.AppendLine("                    break;");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
        }

        private static void AppendCollect(StringBuilder builder, string worldAlias, List<ComponentInfo> components) {
            builder.AppendLine($"        public static void CollectDirty({worldAlias}.Entity e, NetOutbox outbox, NetworkPeerId peer) {{");
            foreach (var component in components) {
                var name = GetTypeName(component.Type);
                builder.AppendLine($"            if (e.Has<{name}>() && e.HasChanged<{name}>())");
                builder.AppendLine($"                outbox.EnqueueComponentDelta(peer, {component.Type.Name}Replication.CreateDelta(e.GID, e.Read<{name}>()), {component.Type.Name}Replication.Delivery);");
            }
            builder.AppendLine("        }");
        }

        private static void AppendCollectClientOwnedDirty(StringBuilder builder, List<ComponentInfo> components) {
            builder.AppendLine("        public static void CollectClientOwnedDirty(NetOutbox outbox, NetworkPeerId peer) {");
            foreach (var component in components) {
                var name = GetTypeName(component.Type);
                builder.AppendLine($"            foreach (var e in CW.Query<All<LocalOwned, NetworkedTag, NetworkIdentity, {name}>, AllChanged<{name}>>().Entities())");
                builder.AppendLine($"                outbox.EnqueueComponentDelta(peer, {component.Type.Name}Replication.CreateDelta(e.GID, e.Read<{name}>()), {component.Type.Name}Replication.Delivery);");
            }
            builder.AppendLine("        }");
        }

        private static void AppendCollectServerOwnedDirty(StringBuilder builder, List<ComponentInfo> components) {
            builder.AppendLine("        public static void CollectServerOwnedDirty(NetOutbox outbox, IReadOnlyList<NetworkPeerId> peers) {");
            foreach (var component in components) {
                var name = GetTypeName(component.Type);
                builder.AppendLine($"            foreach (var e in SW.Query<All<ServerOwned, NetworkedTag, NetworkIdentity, {name}>, AllChanged<{name}>>().Entities()) {{");
                builder.AppendLine("                for (var i = 0; i < peers.Count; i++)");
                builder.AppendLine($"                    outbox.EnqueueComponentDelta(peers[i], {component.Type.Name}Replication.CreateDelta(e.GID, e.Read<{name}>()), {component.Type.Name}Replication.Delivery);");
                builder.AppendLine("            }");
            }
            builder.AppendLine("        }");
        }

        private static void AppendInitialState(StringBuilder builder, List<ComponentInfo> components) {
            builder.AppendLine("        public static void CollectInitialState(SW.Entity e, List<ComponentDelta> components) {");
            builder.AppendLine("            if (e.Has<NetworkIdentity>())");
            builder.AppendLine("                components.Add(NetworkIdentityReplication.CreateDelta(e.GID, e.Read<NetworkIdentity>()));");
            foreach (var component in components) {
                var name = GetTypeName(component.Type);
                builder.AppendLine();
                builder.AppendLine($"            if (e.Has<{name}>())");
                builder.AppendLine($"                components.Add({component.Type.Name}Replication.CreateDelta(e.GID, e.Read<{name}>()));");
            }
            builder.AppendLine("        }");
        }

        private static void AppendWrite(StringBuilder builder, FieldReplicationInfo field) {
            var source = $"state.{field.Field.Name}";
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

        private static void AppendRead(StringBuilder builder, FieldReplicationInfo field) {
            var comma = ",";
            var name = field.Field.Name;

            switch (field.Kind.Name) {
                case "Vector2":
                    builder.AppendLine($"                {name} = new Vector2(reader.ReadFloat(), reader.ReadFloat()){comma}");
                    break;
                case "Vector3":
                    builder.AppendLine($"                {name} = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()){comma}");
                    break;
                case "Quaternion":
                    builder.AppendLine($"                {name} = new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()){comma}");
                    break;
                case "EnumByte":
                    builder.AppendLine($"                {name} = ({GetTypeName(field.Field.FieldType)})reader.ReadByte(){comma}");
                    break;
                case "EnumUshort":
                    builder.AppendLine($"                {name} = ({GetTypeName(field.Field.FieldType)})reader.ReadUshort(){comma}");
                    break;
                case "EnumInt":
                    builder.AppendLine($"                {name} = ({GetTypeName(field.Field.FieldType)})reader.ReadInt(){comma}");
                    break;
                default:
                    builder.AppendLine($"                {name} = reader.{field.Kind.ReadMethod}(){comma}");
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

        private static int EstimateBufferSize(ComponentInfo component) {
            return Math.Max(16, component.Fields.Sum(x => x.Kind.ByteSize));
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
            public readonly NetDelivery Delivery;
            public readonly ushort SendRate;

            public ComponentInfo(Type type, List<FieldReplicationInfo> fields, ushort typeId, ReplicationAuthority authority, NetDelivery delivery, ushort sendRate) {
                Type = type;
                Fields = fields;
                TypeId = typeId;
                Authority = authority;
                Delivery = delivery;
                SendRate = sendRate;
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
            public readonly bool NeedsUnityEngine;
            public bool IsSupported => Name != null;

            private FieldKind(string name, string writeMethod, string readMethod, int byteSize, bool canQuantize = false, bool needsUnityEngine = false) {
                Name = name;
                WriteMethod = writeMethod;
                ReadMethod = readMethod;
                ByteSize = byteSize;
                CanQuantize = canQuantize;
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
                if (type == typeof(float)) return new FieldKind("Float", "WriteFloat", "ReadFloat", 4, canQuantize: true);
                if (type == typeof(Vector2)) return new FieldKind("Vector2", null, null, 8, canQuantize: true, needsUnityEngine: true);
                if (type == typeof(Vector3)) return new FieldKind("Vector3", null, null, 12, canQuantize: true, needsUnityEngine: true);
                if (type == typeof(Quaternion)) return new FieldKind("Quaternion", null, null, 16, canQuantize: true, needsUnityEngine: true);
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
