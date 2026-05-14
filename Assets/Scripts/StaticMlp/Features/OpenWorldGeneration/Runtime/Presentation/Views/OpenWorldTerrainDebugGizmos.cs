using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldTerrainDebugGizmos : MonoBehaviour
    {
        private OpenWorldTerrainRuntime _runtime;

        public void Initialize(OpenWorldTerrainRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        private void OnDrawGizmos()
        {
            if (_runtime == null)
                return;

            var config = _runtime.Config;
            if (!config.ShowDebugGizmos)
                return;
            if (!config.ShowDebugGizmosInEditMode && !Application.isPlaying)
                return;

            var snapshot = _runtime.CreateDebugSnapshot();
            DrawWorldBounds(snapshot, config);
            DrawLoadedChunks(snapshot, config);
        }

        private static void DrawWorldBounds(OpenWorldTerrainDebugSnapshot snapshot, OpenWorldTerrainStreamingConfig config)
        {
            var sizeX = (snapshot.Bounds.MaxX - snapshot.Bounds.MinX + 1) * snapshot.ChunkWorldSize;
            var sizeZ = (snapshot.Bounds.MaxZ - snapshot.Bounds.MinZ + 1) * snapshot.ChunkWorldSize;
            var centerX = (snapshot.Bounds.MinX * snapshot.ChunkWorldSize) + (sizeX * 0.5f);
            var centerZ = (snapshot.Bounds.MinZ * snapshot.ChunkWorldSize) + (sizeZ * 0.5f);
            var center = new Vector3(centerX, config.DebugGizmoHeight * 0.5f, centerZ);

            Gizmos.color = config.DebugWorldBoundsColor;
            Gizmos.DrawWireCube(center, new Vector3(sizeX, config.DebugGizmoHeight, sizeZ));
        }

        private static void DrawLoadedChunks(OpenWorldTerrainDebugSnapshot snapshot, OpenWorldTerrainStreamingConfig config)
        {
            for (var i = 0; i < snapshot.Chunks.Length; i++)
            {
                var chunk = snapshot.Chunks[i];
                var center = chunk.WorldOrigin + new Vector3(
                    snapshot.ChunkWorldSize * 0.5f,
                    config.DebugGizmoHeight * 0.5f,
                    snapshot.ChunkWorldSize * 0.5f);

                Gizmos.color = SelectChunkColor(chunk, config);
                Gizmos.DrawWireCube(center, new Vector3(
                    snapshot.ChunkWorldSize,
                    config.DebugGizmoHeight,
                    snapshot.ChunkWorldSize));

#if UNITY_EDITOR
                if (config.ShowDebugChunkLabels)
                    Handles.Label(center, $"{chunk.ChunkId} LOD{chunk.Lod}" + (chunk.HasCollision ? " C" : string.Empty));
#endif
            }
        }

        private static Color SelectChunkColor(OpenWorldTerrainDebugChunk chunk, OpenWorldTerrainStreamingConfig config)
        {
            if (chunk.HasCollision)
                return config.DebugColliderChunkColor;

            return chunk.Lod switch
            {
                0 => config.DebugLod0ChunkColor,
                1 => config.DebugLod1ChunkColor,
                2 => config.DebugLod2ChunkColor,
                _ => config.DebugLod3ChunkColor
            };
        }
    }
}
