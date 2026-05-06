using System;

namespace StaticMlp.Game.Input
{
    [Serializable]
    public readonly struct InputActionName : IEquatable<InputActionName>
    {
        public readonly string Value;

        public InputActionName(string value)
        {
            Value = value ?? string.Empty;
        }

        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public bool Equals(InputActionName other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is InputActionName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value != null
                ? StringComparer.Ordinal.GetHashCode(Value)
                : 0;
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(InputActionName left, InputActionName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputActionName left, InputActionName right)
        {
            return !left.Equals(right);
        }
    }
}
