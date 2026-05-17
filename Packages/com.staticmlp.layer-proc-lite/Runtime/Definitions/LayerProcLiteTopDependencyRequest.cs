using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteTopDependencyRequest
    {
        public readonly LayerProcLiteLayerId LayerId;
        public readonly int Level;
        public readonly LayerProcLiteWorldBounds Bounds;
        public readonly int Variant;
        public readonly uint SettingsHash;
        public readonly object Settings;

        public LayerProcLiteTopDependencyRequest(
            LayerProcLiteLayerId layerId,
            int level,
            LayerProcLiteWorldBounds bounds,
            int variant = 0,
            uint settingsHash = 0,
            object settings = null)
        {
            if (level < 0)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Top dependency level must be non-negative.");

            LayerId = layerId;
            Level = level;
            Bounds = bounds;
            Variant = variant;
            SettingsHash = settingsHash;
            Settings = settings;
        }

        public static LayerProcLiteTopDependencyRequest FromCenterAndSize(
            LayerProcLiteLayerId layerId,
            int level,
            float centerX,
            float centerZ,
            float sizeX,
            float sizeZ,
            int variant = 0,
            uint settingsHash = 0,
            object settings = null)
        {
            if (sizeX <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sizeX), sizeX, "Top dependency size X must be positive.");
            if (sizeZ <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sizeZ), sizeZ, "Top dependency size Z must be positive.");

            var halfX = sizeX * 0.5f;
            var halfZ = sizeZ * 0.5f;
            return new LayerProcLiteTopDependencyRequest(
                layerId,
                level,
                new LayerProcLiteWorldBounds(centerX - halfX, centerZ - halfZ, centerX + halfX, centerZ + halfZ),
                variant,
                settingsHash,
                settings);
        }
    }
}
