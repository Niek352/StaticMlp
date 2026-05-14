using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldTerrainRuntime : FFS.Libraries.StaticEcs.IResource, IDisposable
    {
        private readonly OpenWorldTerrainStreamingConfig _config;
        private readonly IWorldGenerationService _generationService;
        private readonly TerrainChunkViewFactory _factory;
        private readonly GameObject _root;
        private readonly Dictionary<WorldChunkId, LoadedTerrainChunk> _loadedChunks = new();
        private readonly HashSet<WorldChunkId> _requiredChunks = new();
        private readonly List<WorldChunkId> _chunksToUnload = new();
        private WorldChunkId _focusChunk;
        private bool _hasFocusChunk;

        private OpenWorldTerrainRuntime(
            OpenWorldTerrainStreamingConfig config,
            IWorldGenerationService generationService,
            TerrainChunkViewFactory factory,
            GameObject root)
        {
            _config = config;
            _generationService = generationService;
            _factory = factory;
            _root = root;
        }

        internal OpenWorldTerrainStreamingConfig Config => _config;

        public static OpenWorldTerrainRuntime Create(OpenWorldTerrainStreamingConfig config)
        {
            var root = new GameObject(config.RootName);
            var gizmos = root.AddComponent<OpenWorldTerrainDebugGizmos>();
            var factory = new TerrainChunkViewFactory(root.transform, config.MaterialColor);
            var runtime = new OpenWorldTerrainRuntime(config, new LayerProcGenWorldGenerationService(), factory, root);
            gizmos.Initialize(runtime);
            return runtime;
        }

        public void StreamAround(Vector3 focusPosition)
        {
            _requiredChunks.Clear();

            var rawFocusChunk = WorldChunkId.FromWorldPosition(focusPosition.x, focusPosition.z, _config.ChunkWorldSize);
            var focusChunk = _config.Bounds.Clamp(rawFocusChunk);
            _focusChunk = focusChunk;
            _hasFocusChunk = true;
            var minX = Math.Max(focusChunk.X - _config.ViewRadiusInChunks, _config.Bounds.MinX);
            var maxX = Math.Min(focusChunk.X + _config.ViewRadiusInChunks, _config.Bounds.MaxX);
            var minZ = Math.Max(focusChunk.Z - _config.ViewRadiusInChunks, _config.Bounds.MinZ);
            var maxZ = Math.Min(focusChunk.Z + _config.ViewRadiusInChunks, _config.Bounds.MaxZ);
            var remainingChunkLoads = _config.MaxChunkLoadsPerFrame;

            for (var z = minZ; z <= maxZ; z++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var chunkId = new WorldChunkId(x, z);
                    _requiredChunks.Add(chunkId);
                    if (remainingChunkLoads > 0 && LoadOrUpdateChunk(chunkId, focusChunk))
                        remainingChunkLoads--;
                }
            }

            UnloadUnneededChunks();
        }

        public OpenWorldTerrainDebugSnapshot CreateDebugSnapshot()
        {
            var chunks = new List<OpenWorldTerrainDebugChunk>(_loadedChunks.Count);
            foreach (var pair in _loadedChunks)
            {
                chunks.Add(new OpenWorldTerrainDebugChunk(
                    pair.Key,
                    pair.Value.Lod,
                    pair.Value.Collision,
                    pair.Key.GetWorldOrigin(_config.ChunkWorldSize)));
            }

            chunks.Sort(CompareDebugChunks);

            return new OpenWorldTerrainDebugSnapshot(
                _config.Seed,
                _config.Bounds,
                _config.ChunkWorldSize,
                _hasFocusChunk,
                _focusChunk,
                chunks.ToArray());
        }

        public void Dispose()
        {
            foreach (var pair in _loadedChunks)
                pair.Value.View.Dispose();

            _loadedChunks.Clear();
            _requiredChunks.Clear();
            _chunksToUnload.Clear();
            _factory.Dispose();
            if (_generationService is IDisposable disposable)
                disposable.Dispose();
            DestroyOwnedObject(_root);
        }

        private bool LoadOrUpdateChunk(WorldChunkId chunkId, WorldChunkId focusChunk)
        {
            var lod = TerrainLodSelector.SelectLod(chunkId, focusChunk);
            var collision = Math.Max(Math.Abs(chunkId.X - focusChunk.X), Math.Abs(chunkId.Z - focusChunk.Z)) <= _config.ColliderRadiusInChunks;

            if (_loadedChunks.TryGetValue(chunkId, out var loaded)
                && loaded.Lod == lod
                && loaded.Collision == collision)
                return false;

            var request = _config.CreateGenerationRequest(lod);
            var generated = _generationService.GenerateChunk(chunkId, request);

            if (!_loadedChunks.TryGetValue(chunkId, out loaded))
                loaded = new LoadedTerrainChunk(_factory.Create(chunkId, _config.ChunkWorldSize), lod, collision);

            loaded.View.Apply(chunkId, lod, generated.TerrainMesh, collision);
            _loadedChunks[chunkId] = new LoadedTerrainChunk(loaded.View, lod, collision);
            return true;
        }

        private void UnloadUnneededChunks()
        {
            _chunksToUnload.Clear();
            _chunksToUnload.AddRange(_loadedChunks.Keys);

            foreach (var chunkId in _chunksToUnload)
            {
                if (_requiredChunks.Contains(chunkId))
                    continue;

                _loadedChunks[chunkId].View.Dispose();
                _loadedChunks.Remove(chunkId);
            }

            _chunksToUnload.Clear();
        }

        private static int CompareDebugChunks(OpenWorldTerrainDebugChunk left, OpenWorldTerrainDebugChunk right)
        {
            var zComparison = left.ChunkId.Z.CompareTo(right.ChunkId.Z);
            return zComparison != 0 ? zComparison : left.ChunkId.X.CompareTo(right.ChunkId.X);
        }

        private static void DestroyOwnedObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        private readonly struct LoadedTerrainChunk
        {
            public LoadedTerrainChunk(TerrainChunkView view, int lod, bool collision)
            {
                View = view;
                Lod = lod;
                Collision = collision;
            }

            public readonly TerrainChunkView View;
            public readonly int Lod;
            public readonly bool Collision;
        }
    }
}
