using System;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcIncubationRecipeId : IEquatable<NpcIncubationRecipeId>
    {
        public readonly ushort Value;

        public NpcIncubationRecipeId(ushort value)
        {
            Value = value;
        }

        public bool Equals(NpcIncubationRecipeId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is NpcIncubationRecipeId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(NpcIncubationRecipeId left, NpcIncubationRecipeId right) => left.Equals(right);
        public static bool operator !=(NpcIncubationRecipeId left, NpcIncubationRecipeId right) => !left.Equals(right);
    }
}
