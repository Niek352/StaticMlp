using System;

namespace StaticMlp.Features.Settlement
{
    public readonly struct WorkbenchRecipeId : IEquatable<WorkbenchRecipeId>
    {
        public readonly ushort Value;

        public WorkbenchRecipeId(ushort value)
        {
            Value = value;
        }

        public bool Equals(WorkbenchRecipeId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is WorkbenchRecipeId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(WorkbenchRecipeId left, WorkbenchRecipeId right) => left.Equals(right);
        public static bool operator !=(WorkbenchRecipeId left, WorkbenchRecipeId right) => !left.Equals(right);
    }
}
