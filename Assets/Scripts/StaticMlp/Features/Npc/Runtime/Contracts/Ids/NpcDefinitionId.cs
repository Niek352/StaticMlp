using System;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcDefinitionId : IEquatable<NpcDefinitionId>
    {
        public readonly ushort Value;

        public NpcDefinitionId(ushort value)
        {
            Value = value;
        }

        public bool Equals(NpcDefinitionId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is NpcDefinitionId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(NpcDefinitionId left, NpcDefinitionId right) => left.Equals(right);
        public static bool operator !=(NpcDefinitionId left, NpcDefinitionId right) => !left.Equals(right);
    }
}
