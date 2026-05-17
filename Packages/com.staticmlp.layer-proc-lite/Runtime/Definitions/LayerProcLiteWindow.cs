using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteWindow : IEquatable<LayerProcLiteWindow>
    {
        public static readonly LayerProcLiteWindow None = new(0, 0f);

        public readonly int PaddingSamples;
        public readonly float EffectDistanceWorld;

        public LayerProcLiteWindow(int paddingSamples, float effectDistanceWorld)
        {
            if (paddingSamples < 0)
                throw new ArgumentOutOfRangeException(nameof(paddingSamples), paddingSamples, "Padding samples must be non-negative.");
            if (effectDistanceWorld < 0f)
                throw new ArgumentOutOfRangeException(nameof(effectDistanceWorld), effectDistanceWorld, "Effect distance must be non-negative.");

            PaddingSamples = paddingSamples;
            EffectDistanceWorld = effectDistanceWorld;
        }

        public LayerProcLiteWindow ExpandedBy(int paddingSamples, float effectDistanceWorld)
        {
            if (paddingSamples < 0)
                throw new ArgumentOutOfRangeException(nameof(paddingSamples), paddingSamples, "Padding samples must be non-negative.");
            if (effectDistanceWorld < 0f)
                throw new ArgumentOutOfRangeException(nameof(effectDistanceWorld), effectDistanceWorld, "Effect distance must be non-negative.");

            return new LayerProcLiteWindow(PaddingSamples + paddingSamples, EffectDistanceWorld + effectDistanceWorld);
        }

        public static LayerProcLiteWindow Max(LayerProcLiteWindow left, LayerProcLiteWindow right)
        {
            return new LayerProcLiteWindow(
                Math.Max(left.PaddingSamples, right.PaddingSamples),
                Math.Max(left.EffectDistanceWorld, right.EffectDistanceWorld));
        }

        public bool Equals(LayerProcLiteWindow other)
        {
            return PaddingSamples == other.PaddingSamples
                   && EffectDistanceWorld.Equals(other.EffectDistanceWorld);
        }

        public override bool Equals(object obj)
        {
            return obj is LayerProcLiteWindow other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PaddingSamples * 397) ^ EffectDistanceWorld.GetHashCode();
            }
        }
    }
}
