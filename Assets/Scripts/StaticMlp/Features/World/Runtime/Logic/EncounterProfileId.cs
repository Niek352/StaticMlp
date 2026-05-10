using System;

namespace StaticMlp.Features.World
{
    public readonly struct EncounterProfileId : IEquatable<EncounterProfileId>
    {
        public readonly ushort Value;

        public EncounterProfileId(ushort value)
        {
            Value = value;
        }

        public bool Equals(EncounterProfileId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is EncounterProfileId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(EncounterProfileId left, EncounterProfileId right) => left.Equals(right);
        public static bool operator !=(EncounterProfileId left, EncounterProfileId right) => !left.Equals(right);
    }
}
