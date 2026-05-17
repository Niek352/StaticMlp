using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    [Flags]
    public enum GenerationOutputMask : byte
    {
        None = 0,
        Placements = 1 << 0,
        VisualMesh = 1 << 1,
        PhysicsMesh = 1 << 2,
        NavMeshSourceMesh = 1 << 3,
        ServerGeometry = PhysicsMesh | NavMeshSourceMesh,
        All = Placements | VisualMesh | PhysicsMesh | NavMeshSourceMesh,

        [Obsolete("Use VisualMesh, PhysicsMesh, or NavMeshSourceMesh explicitly.")]
        Mesh = VisualMesh
    }
}
