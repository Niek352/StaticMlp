using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteDependency
    {
        public readonly LayerProcLiteLayerId ProviderLayerId;
        public readonly int UserLevel;
        public readonly int ProviderLevel;
        public readonly int PaddingSamples;
        public readonly float EffectDistanceWorld;

        public LayerProcLiteLayerId LayerId => ProviderLayerId;
        public int InternalLevel => ProviderLevel;
        public float PaddingWorld => EffectDistanceWorld;

        public LayerProcLiteDependency(
            LayerProcLiteLayerId providerLayerId,
            int userLevel,
            int providerLevel,
            int paddingSamples,
            float effectDistanceWorld)
        {
            if (userLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(userLevel), userLevel, "User level must be non-negative.");
            if (providerLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(providerLevel), providerLevel, "Provider level must be non-negative.");
            if (paddingSamples < 0)
                throw new ArgumentOutOfRangeException(nameof(paddingSamples), paddingSamples, "Padding samples must be non-negative.");
            if (effectDistanceWorld < 0f)
                throw new ArgumentOutOfRangeException(nameof(effectDistanceWorld), effectDistanceWorld, "Effect distance must be non-negative.");

            ProviderLayerId = providerLayerId;
            UserLevel = userLevel;
            ProviderLevel = providerLevel;
            PaddingSamples = paddingSamples;
            EffectDistanceWorld = effectDistanceWorld;
        }

        public LayerProcLiteDependency(
            LayerProcLiteLayerId layerId,
            int internalLevel,
            int paddingSamples,
            float effectDistanceWorld)
            : this(layerId, 0, internalLevel, paddingSamples, effectDistanceWorld)
        {
        }

        public LayerProcLiteDependency(
            LayerProcLiteLayerId layerId,
            int paddingSamples,
            float effectDistanceWorld)
            : this(layerId, 0, 0, paddingSamples, effectDistanceWorld)
        {
        }
    }
}
