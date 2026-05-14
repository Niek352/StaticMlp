using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct WorldGenerationSeed : IEquatable<WorldGenerationSeed>
    {
        public readonly int Value;

        public WorldGenerationSeed(int value)
        {
            Value = value;
        }

        public bool Equals(WorldGenerationSeed other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldGenerationSeed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(WorldGenerationSeed left, WorldGenerationSeed right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldGenerationSeed left, WorldGenerationSeed right)
        {
            return !left.Equals(right);
        }
    }
}
