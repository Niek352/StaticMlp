using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;
using UnityCodeGen;
using UnityEditor;
using UnityEngine;

namespace StaticMlp.Editor.ReplicationCodeGen
{
    [Generator]
    public sealed class ReplicatedEventCodeGenerator : ICodeGenerator
    {
        private const string OutputFolder = "Assets/Scripts/StaticMlp/Generated/ReplicationEvents";

        public void Execute(GeneratorContext context)
        {
            context.OverrideFolderPath(OutputFolder);

            var diagnostics = new List<string>();
            var events = TypeCache.GetTypesWithAttribute<ReplicatedEventAttribute>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .Select(t => CreateEventInfo(t, diagnostics))
                .Where(x => x != null)
                .ToList();

            ValidateDuplicateIds(events, diagnostics);

            context.AddCode("StaticMlp.ReplicatedEvents.CodeGenDiagnostics.Generated.cs", EmitDiagnostics(diagnostics));
            if (diagnostics.Count != 0)
                return;

            context.AddCode("ReplicatedNetworkEventIds.Generated.cs", EmitIds(events));
            context.AddCode("ReplicatedNetworkEventRegistry.Generated.cs", EmitRegistry(events));
        }

        private static EventInfo CreateEventInfo(Type type, List<string> diagnostics)
        {
            var attribute = type.GetCustomAttribute<ReplicatedEventAttribute>();
            if (attribute == null)
                return null;

            if (!type.IsValueType || type.IsEnum)
                diagnostics.Add($"{type.FullName}: replicated event must be a struct.");

            if (!typeof(IEvent).IsAssignableFrom(type))
                diagnostics.Add($"{type.FullName}: replicated event must implement IEvent.");

            var explicitId = TryReadExplicitEventId(type);
            var guid = TryReadStaticEcsGuid(type);
            if (!explicitId.HasValue && !guid.HasValue)
                diagnostics.Add($"{type.FullName}: replicated event must expose NETWORK_EVENT_ID or a stable StaticEcs guid through IEventConfig<T>.Config().");

            var members = GetReplicatedMembers(type)
                .Select(m => MemberInfoFor(m, diagnostics))
                .Where(x => x != null)
                .ToList();

            if (members.Count == 0)
                diagnostics.Add($"{type.FullName}: replicated event has no public replicated fields or properties.");

            return new EventInfo(type, members, GetEventId(type, explicitId, guid), attribute.Delivery);
        }

        private static Guid? TryReadStaticEcsGuid(Type type)
        {
            try
            {
                var configMethod = type.GetMethod("Config", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                if (configMethod == null)
                    return null;

                var evt = Activator.CreateInstance(type);
                var config = configMethod.Invoke(evt, Array.Empty<object>());
                var guidField = config?.GetType().GetField("Guid", BindingFlags.Instance | BindingFlags.Public);
                return guidField?.GetValue(config) as Guid?;
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<MemberInfo> GetReplicatedMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => IsReplicatedMember(m))
                .OrderBy(m => m.MetadataToken);
        }

        private static bool IsReplicatedMember(MemberInfo member)
        {
            if (member is FieldInfo field)
                return !field.IsStatic;

            if (member is PropertyInfo property)
                return property.GetIndexParameters().Length == 0
                       && property.GetMethod != null
                       && !property.GetMethod.IsStatic
                       && property.SetMethod != null
                       && !property.SetMethod.IsStatic;

            return false;
        }

        private static EventMemberInfo MemberInfoFor(MemberInfo member, List<string> diagnostics)
        {
            var memberType = GetMemberType(member);
            var kind = FieldKind.From(memberType);
            if (!kind.IsSupported)
            {
                diagnostics.Add($"{member.DeclaringType.FullName}.{member.Name}: unsupported replicated event member type {memberType.FullName}.");
                return null;
            }

            return new EventMemberInfo(member, kind);
        }

        private static Type GetMemberType(MemberInfo member)
        {
            if (member is FieldInfo field)
                return field.FieldType;

            if (member is PropertyInfo property)
                return property.PropertyType;

            throw new ArgumentException($"Unsupported replicated event member {member.Name}.");
        }

        private static ushort? TryReadExplicitEventId(Type type)
        {
            var explicitId = type.GetField("NETWORK_EVENT_ID", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (explicitId != null && explicitId.FieldType == typeof(ushort) && explicitId.IsLiteral)
                return (ushort)explicitId.GetRawConstantValue();

            return null;
        }

        private static ushort GetEventId(Type type, ushort? explicitId, Guid? guid)
        {
            if (explicitId.HasValue)
                return explicitId.Value;

            var source = guid?.ToString("N") ?? type.FullName;
            var hash = 2166136261u;
            foreach (var ch in source)
            {
                hash ^= ch;
                hash *= 16777619u;
            }

            return (ushort)(1 + hash % 65534);
        }

        private static void ValidateDuplicateIds(List<EventInfo> events, List<string> diagnostics)
        {
            foreach (var group in events.GroupBy(x => x.TypeId).Where(g => g.Count() > 1))
            {
                var names = string.Join(", ", group.Select(x => x.Type.FullName));
                diagnostics.Add($"Duplicate replicated event type id {group.Key}: {names}.");
            }
        }

        private static string EmitDiagnostics(List<string> diagnostics)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine("    public static class ReplicatedEventsCodeGenDiagnostics { }");
            builder.AppendLine("}");
            foreach (var diagnostic in diagnostics)
                builder.AppendLine($"#error {EscapeError(diagnostic)}");
            return builder.ToString();
        }

        private static string EmitIds(List<EventInfo> events)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine("    public static class ReplicatedNetworkEventIds {");
            foreach (var evt in events)
                builder.AppendLine($"        public const ushort {evt.Type.Name} = {evt.TypeId};");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string EmitRegistry(List<EventInfo> events)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine("using FFS.Libraries.StaticEcs;");
            builder.AppendLine("using StaticMlp.Networking;");
            builder.AppendLine("using StaticMlp.Networking.Replication;");
            foreach (var ns in GetRequiredNamespaces(events))
                builder.AppendLine($"using {ns};");
            if (events.Any(x => x.Members.Any(f => f.Kind.NeedsUnityEngine)))
                builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine("namespace StaticMlp.Networking.Replication.Generated {");
            builder.AppendLine("    public static class ReplicatedNetworkEventRegistry {");
            builder.AppendLine("        public static void RegisterNetworkEvents() {");
            foreach (var evt in events)
            {
                var name = evt.Type.Name;
                builder.AppendLine($"            NetworkEventRegistry.Register<{name}>(");
                builder.AppendLine($"                ReplicatedNetworkEventIds.{name},");
                builder.AppendLine($"                NetDelivery.{evt.Delivery},");
                builder.AppendLine($"                {EstimateBufferSize(evt)},");
                builder.AppendLine($"                Write{name},");
                builder.AppendLine($"                Read{name});");
            }
            builder.AppendLine("        }");
            foreach (var evt in events)
            {
                builder.AppendLine();
                AppendWriteMethod(builder, evt);
                builder.AppendLine();
                AppendReadMethod(builder, evt);
            }
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static IEnumerable<string> GetRequiredNamespaces(IEnumerable<EventInfo> events)
        {
            return events
                .SelectMany(evt => evt.Members
                    .Select(member => GetMemberType(member.Member).Namespace)
                    .Concat(new[] { evt.Type.Namespace }))
                .Where(ns => !string.IsNullOrEmpty(ns) && ns != "UnityEngine")
                .Distinct()
                .OrderBy(ns => ns, StringComparer.Ordinal);
        }

        private static void AppendWriteMethod(StringBuilder builder, EventInfo evt)
        {
            var name = evt.Type.Name;
            builder.AppendLine($"        private static void Write{name}(ref NetworkWriter writer, in {name} evt) {{");
            foreach (var member in evt.Members)
                AppendWrite(builder, member);
            builder.AppendLine("        }");
        }

        private static void AppendReadMethod(StringBuilder builder, EventInfo evt)
        {
            var name = evt.Type.Name;
            builder.AppendLine($"        private static {name} Read{name}(ref NetworkReader reader) {{");
            builder.AppendLine($"            return new {name} {{");
            foreach (var member in evt.Members)
                AppendRead(builder, member);
            builder.AppendLine("            };");
            builder.AppendLine("        }");
        }

        private static void AppendWrite(StringBuilder builder, EventMemberInfo member)
        {
            var source = $"evt.{member.Member.Name}";

            switch (member.Kind.Name)
            {
                case "Vector2":
                    builder.AppendLine($"            writer.WriteFloat({source}.x, {source}.y);");
                    break;
                case "Vector3":
                    builder.AppendLine($"            writer.WriteFloat({source}.x, {source}.y, {source}.z);");
                    break;
                case "Quaternion":
                    builder.AppendLine($"            writer.WriteFloat({source}.x, {source}.y, {source}.z, {source}.w);");
                    break;
                case "EntityGID":
                    builder.AppendLine($"            writer.WriteEntityGid({source});");
                    break;
                case "RequestId":
                    builder.AppendLine($"            writer.WriteRequestId({source});");
                    break;
                case "WorldChunkId":
                    builder.AppendLine($"            writer.WriteWorldChunkId({source});");
                    break;
                case "UshortValueObject":
                    builder.AppendLine($"            writer.WriteUshort({source}.Value);");
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
                    builder.AppendLine($"            writer.{member.Kind.WriteMethod}({source});");
                    break;
            }
        }

        private static void AppendRead(StringBuilder builder, EventMemberInfo member)
        {
            var name = member.Member.Name;

            switch (member.Kind.Name)
            {
                case "Vector2":
                    builder.AppendLine($"                {name} = reader.ReadVector2(),");
                    break;
                case "Vector3":
                    builder.AppendLine($"                {name} = reader.ReadVector3(),");
                    break;
                case "Quaternion":
                    builder.AppendLine($"                {name} = reader.ReadQuaternion(),");
                    break;
                case "EntityGID":
                    builder.AppendLine($"                {name} = reader.ReadEntityGid(),");
                    break;
                case "RequestId":
                    builder.AppendLine($"                {name} = reader.ReadRequestId(),");
                    break;
                case "WorldChunkId":
                    builder.AppendLine($"                {name} = reader.ReadWorldChunkId(),");
                    break;
                case "UshortValueObject":
                    builder.AppendLine($"                {name} = new {GetTypeName(GetMemberType(member.Member))}(reader.ReadUshort()),");
                    break;
                case "EnumByte":
                    builder.AppendLine($"                {name} = ({GetTypeName(GetMemberType(member.Member))})reader.ReadByte(),");
                    break;
                case "EnumUshort":
                    builder.AppendLine($"                {name} = ({GetTypeName(GetMemberType(member.Member))})reader.ReadUshort(),");
                    break;
                case "EnumInt":
                    builder.AppendLine($"                {name} = ({GetTypeName(GetMemberType(member.Member))})reader.ReadInt(),");
                    break;
                default:
                    builder.AppendLine($"                {name} = reader.{member.Kind.ReadMethod}(),");
                    break;
            }
        }

        private static int EstimateBufferSize(EventInfo evt)
        {
            return Math.Max(16, evt.Members.Sum(x => x.Kind.ByteSize));
        }

        private static string GetTypeName(Type type)
        {
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

        private static string Camel(string text)
        {
            return string.IsNullOrEmpty(text)
                ? text
                : char.ToLowerInvariant(text[0]) + text.Substring(1);
        }

        private static string EscapeError(string text)
        {
            return text.Replace("\\", "/").Replace("\r", " ").Replace("\n", " ");
        }

        private sealed class EventInfo
        {
            public readonly Type Type;
            public readonly List<EventMemberInfo> Members;
            public readonly ushort TypeId;
            public readonly NetDelivery Delivery;

            public EventInfo(Type type, List<EventMemberInfo> members, ushort typeId, NetDelivery delivery)
            {
                Type = type;
                Members = members;
                TypeId = typeId;
                Delivery = delivery;
            }
        }

        private sealed class EventMemberInfo
        {
            public readonly MemberInfo Member;
            public readonly FieldKind Kind;

            public EventMemberInfo(MemberInfo member, FieldKind kind)
            {
                Member = member;
                Kind = kind;
            }
        }

        private readonly struct FieldKind
        {
            public readonly string Name;
            public readonly string WriteMethod;
            public readonly string ReadMethod;
            public readonly int ByteSize;
            public readonly bool NeedsUnityEngine;
            public bool IsSupported => Name != null;

            private FieldKind(string name, string writeMethod, string readMethod, int byteSize, bool needsUnityEngine = false)
            {
                Name = name;
                WriteMethod = writeMethod;
                ReadMethod = readMethod;
                ByteSize = byteSize;
                NeedsUnityEngine = needsUnityEngine;
            }

            public static FieldKind From(Type type)
            {
                if (type == typeof(bool)) return new FieldKind("Bool", "WriteBool", "ReadBool", 1);
                if (type == typeof(byte)) return new FieldKind("Byte", "WriteByte", "ReadByte", 1);
                if (type == typeof(sbyte)) return new FieldKind("Sbyte", "WriteSbyte", "ReadSbyte", 1);
                if (type == typeof(short)) return new FieldKind("Short", "WriteShort", "ReadShort", 2);
                if (type == typeof(ushort)) return new FieldKind("Ushort", "WriteUshort", "ReadUshort", 2);
                if (type == typeof(int)) return new FieldKind("Int", "WriteInt", "ReadInt", 4);
                if (type == typeof(uint)) return new FieldKind("Uint", "WriteUint", "ReadUint", 4);
                if (type == typeof(long)) return new FieldKind("Long", "WriteLong", "ReadLong", 8);
                if (type == typeof(ulong)) return new FieldKind("Ulong", "WriteUlong", "ReadUlong", 8);
                if (type == typeof(float)) return new FieldKind("Float", "WriteFloat", "ReadFloat", 4);
                if (type == typeof(EntityGID)) return new FieldKind("EntityGID", null, null, 8);
                if (type == typeof(RequestId)) return new FieldKind("RequestId", null, null, 4);
                if (type == typeof(WorldChunkId)) return new FieldKind("WorldChunkId", null, null, 8);
                if (IsUshortValueObject(type)) return new FieldKind("UshortValueObject", null, null, 2);
                if (type == typeof(Vector2)) return new FieldKind("Vector2", null, null, 8, needsUnityEngine: true);
                if (type == typeof(Vector3)) return new FieldKind("Vector3", null, null, 12, needsUnityEngine: true);
                if (type == typeof(Quaternion)) return new FieldKind("Quaternion", null, null, 16, needsUnityEngine: true);
                if (type.IsEnum)
                {
                    var underlying = Enum.GetUnderlyingType(type);
                    if (underlying == typeof(byte)) return new FieldKind("EnumByte", null, null, 1);
                    if (underlying == typeof(ushort)) return new FieldKind("EnumUshort", null, null, 2);
                    if (underlying == typeof(int)) return new FieldKind("EnumInt", null, null, 4);
                }
                return default;
            }

            private static bool IsUshortValueObject(Type type) {
                if (type.FullName != "StaticMlp.Features.Settlement.SettlementAnchorId" &&
                    type.FullName != "StaticMlp.Features.Loadout.LoadoutModuleId")
                    return false;

                var value = type.GetField("Value", BindingFlags.Instance | BindingFlags.Public);
                var constructor = type.GetConstructor(new[] { typeof(ushort) });
                return value != null && value.FieldType == typeof(ushort) && constructor != null;
            }
        }
    }
}
