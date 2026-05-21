using System;
using Unity.Mathematics;
using UnityEngine;

namespace StaticMlp.Features.AiNavigation
{
    public static class RuntimeNavMeshZoneBounds
    {
        private const float NAV_MESH_BOUNDS_Y = 1000f;

        public static Bounds Create(float3 center, float radius)
        {
            if (!math.all(math.isfinite(center)))
                throw new InvalidOperationException("Runtime NavMesh zone center must be finite.");
            if (!math.isfinite(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Runtime NavMesh zone radius must be positive.");

            return new Bounds(
                new Vector3(center.x, center.y, center.z),
                new Vector3(radius * 2f, NAV_MESH_BOUNDS_Y, radius * 2f));
        }
    }
}
