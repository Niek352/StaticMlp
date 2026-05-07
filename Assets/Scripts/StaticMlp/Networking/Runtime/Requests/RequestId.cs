using System;

namespace StaticMlp.Networking.Requests
{
    public readonly struct RequestId : IEquatable<RequestId>
    {
        public readonly uint Value;

        public RequestId(uint value)
        {
            Value = value;
        }

        public bool Equals(RequestId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is RequestId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public static bool operator ==(RequestId left, RequestId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RequestId left, RequestId right)
        {
            return !left.Equals(right);
        }
    }
}
