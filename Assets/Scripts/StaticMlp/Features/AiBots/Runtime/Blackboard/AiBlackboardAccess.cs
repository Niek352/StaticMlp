using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public static class AiBlackboardAccess
    {
        public static void SetFloat(SW.Entity entity, ushort variableId, float value)
        {
            ref var entries = ref EnsureEntries(entity);
            var index = FindIndex(in entries, variableId);
            if (index >= 0)
            {
                ref var entry = ref entries[index];
                entry.VariableId = variableId;
                entry.Kind = AiBlackboardValueKind.Float;
                entry.FloatValue = value;
                entry.EntityValue = default;
                entry.VectorValue = default;
                return;
            }

            entries.Add(new AiBlackboardEntry
            {
                VariableId = variableId,
                Kind = AiBlackboardValueKind.Float,
                FloatValue = value
            });
        }

        public static void SetEntity(SW.Entity entity, ushort variableId, EntityGID value)
        {
            if (!value.TryUnpack<ServerWT>(out _))
            {
                Remove(entity, variableId);
                return;
            }

            ref var entries = ref EnsureEntries(entity);
            var index = FindIndex(in entries, variableId);
            if (index >= 0)
            {
                ref var entry = ref entries[index];
                entry.VariableId = variableId;
                entry.Kind = AiBlackboardValueKind.Entity;
                entry.FloatValue = 0f;
                entry.EntityValue = value;
                entry.VectorValue = default;
                return;
            }

            entries.Add(new AiBlackboardEntry
            {
                VariableId = variableId,
                Kind = AiBlackboardValueKind.Entity,
                EntityValue = value
            });
        }

        public static void SetVector(SW.Entity entity, ushort variableId, Vector3 value)
        {
            ref var entries = ref EnsureEntries(entity);
            var index = FindIndex(in entries, variableId);
            if (index >= 0)
            {
                ref var entry = ref entries[index];
                entry.VariableId = variableId;
                entry.Kind = AiBlackboardValueKind.Vector3;
                entry.FloatValue = 0f;
                entry.EntityValue = default;
                entry.VectorValue = value;
                return;
            }

            entries.Add(new AiBlackboardEntry
            {
                VariableId = variableId,
                Kind = AiBlackboardValueKind.Vector3,
                VectorValue = value
            });
        }

        public static bool TryGetFloat(SW.Entity entity, ushort variableId, out float value)
        {
            if (TryGetEntry(entity, variableId, out var entry)
                && entry.Kind == AiBlackboardValueKind.Float)
            {
                value = entry.FloatValue;
                return true;
            }

            value = default;
            return false;
        }

        public static float GetFloat(SW.Entity entity, ushort variableId, float defaultValue = 0f)
        {
            return TryGetFloat(entity, variableId, out var value) ? value : defaultValue;
        }

        public static bool TryGetEntity(SW.Entity entity, ushort variableId, out EntityGID value)
        {
            if (TryGetEntry(entity, variableId, out var entry)
                && entry.Kind == AiBlackboardValueKind.Entity
                && entry.EntityValue.TryUnpack<ServerWT>(out _))
            {
                value = entry.EntityValue;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetVector(SW.Entity entity, ushort variableId, out Vector3 value)
        {
            if (TryGetEntry(entity, variableId, out var entry)
                && entry.Kind == AiBlackboardValueKind.Vector3)
            {
                value = entry.VectorValue;
                return true;
            }

            value = default;
            return false;
        }

        public static bool Has(SW.Entity entity, ushort variableId)
        {
            if (!entity.Has<SW.Multi<AiBlackboardEntry>>())
                return false;

            ref var entries = ref entity.Ref<SW.Multi<AiBlackboardEntry>>();
            return FindIndex(in entries, variableId) >= 0;
        }

        public static void Remove(SW.Entity entity, ushort variableId)
        {
            if (!entity.Has<SW.Multi<AiBlackboardEntry>>())
                return;

            ref var entries = ref entity.Ref<SW.Multi<AiBlackboardEntry>>();
            var index = FindIndex(in entries, variableId);
            if (index >= 0)
                entries.RemoveAt(index);
        }

        private static ref SW.Multi<AiBlackboardEntry> EnsureEntries(SW.Entity entity) => ref entity.Add<SW.Multi<AiBlackboardEntry>>();

        private static bool TryGetEntry(SW.Entity entity, ushort variableId, out AiBlackboardEntry entry)
        {
            if (!entity.Has<SW.Multi<AiBlackboardEntry>>())
            {
                entry = default;
                return false;
            }

            ref var entries = ref entity.Ref<SW.Multi<AiBlackboardEntry>>();
            var index = FindIndex(in entries, variableId);
            if (index < 0)
            {
                entry = default;
                return false;
            }

            entry = entries[index];
            return true;
        }

        private static int FindIndex(in SW.Multi<AiBlackboardEntry> entries, ushort variableId)
        {
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].VariableId == variableId)
                    return i;
            }

            return -1;
        }
    }
}
