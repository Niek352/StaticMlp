using System;

namespace StaticMlp.Networking {
    public readonly struct NetworkPeerId : IEquatable<NetworkPeerId> {
        public readonly ushort Value;

        public NetworkPeerId(ushort value) {
            Value = value;
        }

        public bool Equals(NetworkPeerId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is NetworkPeerId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();

        public static bool operator ==(NetworkPeerId left, NetworkPeerId right) => left.Equals(right);
        public static bool operator !=(NetworkPeerId left, NetworkPeerId right) => !left.Equals(right);
    }
}
