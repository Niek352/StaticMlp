using System;

namespace StaticMlp.Features.Settlement
{
    public readonly struct ProductionRecipeId : IEquatable<ProductionRecipeId>
    {
        public readonly ushort Value;

        public ProductionRecipeId(ushort value)
        {
            Value = value;
        }

        public bool Equals(ProductionRecipeId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ProductionRecipeId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ProductionRecipeId left, ProductionRecipeId right) => left.Equals(right);
        public static bool operator !=(ProductionRecipeId left, ProductionRecipeId right) => !left.Equals(right);
    }
}
