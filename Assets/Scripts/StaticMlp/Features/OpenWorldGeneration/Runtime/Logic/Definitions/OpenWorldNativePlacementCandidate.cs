using Unity.Mathematics;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct OpenWorldNativePlacementCandidate
    {
        public static readonly OpenWorldNativePlacementCandidate Invalid = new(false, float3.zero, 0f, 1f, default);

        public readonly bool Valid;
        public readonly float3 Position;
        public readonly float YawDegrees;
        public readonly float Scale;
        public readonly OpenWorldNativeSurfaceSample Surface;

        public OpenWorldNativePlacementCandidate(
            bool valid,
            float3 position,
            float yawDegrees,
            float scale,
            OpenWorldNativeSurfaceSample surface)
        {
            Valid = valid;
            Position = position;
            YawDegrees = yawDegrees;
            Scale = scale;
            Surface = surface;
        }
    }
}
