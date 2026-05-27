using System;

namespace StaticMlp.Features.Settlement
{
    public readonly struct ProductionStationId : IEquatable<ProductionStationId>
    {
        public readonly ushort Value;

        public ProductionStationId(ushort value)
        {
            Value = value;
        }

        public bool Equals(ProductionStationId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ProductionStationId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ProductionStationId left, ProductionStationId right) => left.Equals(right);
        public static bool operator !=(ProductionStationId left, ProductionStationId right) => !left.Equals(right);
    }
}
