using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldTerrainRuntime : IResource, IDisposable
    {
        private readonly OpenWorldTerrainStreamingConfig _config;
        private readonly TerrainChunkViewFactory _factory;
        private readonly GameObject _root;
        private readonly Dictionary<WorldChunkId, LoadedTerrainChunk> _loadedChunks = new();
        private readonly HashSet<WorldChunkId> _requiredChunks = new();
        private readonly List<WorldChunkId> _chunksToUnload = new();
        private readonly List<PendingTerrainMeshKey> _pendingMeshKeysToRemove = new();
        private readonly Dictionary<PendingTerrainMeshKey, TerrainMeshData> _pendingMeshes = new();
        private WorldChunkId _focusChunk;
        private bool _hasFocusChunk;
        private int _loadRequestsThisFrame;

        private OpenWorldTerrainRuntime(
            OpenWorldTerrainStreamingConfig config,
            TerrainChunkViewFactory factory,
            GameObject root)
        {
            _config = config;
            _factory = factory;
            _root = root;
        }

        internal OpenWorldTerrainStreamingConfig Config => _config;

        public static OpenWorldTerrainRuntime Create(OpenWorldTerrainStreamingConfig config)
        {
            var root = new GameObject(config.RootName);
            var gizmos = root.AddComponent<OpenWorldTerrainDebugGizmos>();
            var factory = new TerrainChunkViewFactory(root.transform, config.MaterialColor);
            var runtime = new OpenWorldTerrainRuntime(config, factory, root);
            gizmos.Initialize(runtime);
            return runtime;
        }

        public void StreamAround(Vector3 focusPosition)
        {
            _requiredChunks.Clear();
            _loadRequestsThisFrame = 0;

            var rawFocusChunk = WorldChunkId.FromWorldPosition(focusPosition.x, focusPosition.z, _config.ChunkWorldSize);
            var focusChunk = _config.Bounds.Clamp(rawFocusChunk);
            _focusChunk = focusChunk;
            _hasFocusChunk = true;
            var minX = Math.Max(focusChunk.X - _config.ViewRadiusInChunks, _config.Bounds.MinX);
            var maxX = Math.Min(focusChunk.X + _config.ViewRadiusInChunks, _config.Bounds.MaxX);
            var minZ = Math.Max(focusChunk.Z - _config.ViewRadiusInChunks, _config.Bounds.MinZ);
            var maxZ = Math.Min(focusChunk.Z + _config.ViewRadiusInChunks, _config.Bounds.MaxZ);

            for (var z = minZ; z <= maxZ; z++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var chunkId = new WorldChunkId(x, z);
                    _requiredChunks.Add(chunkId);
                    LoadOrUpdateChunk(chunkId, focusChunk);
                }
            }

            UnloadUnneededChunks();
        }

        public void ApplyChunkMesh(WorldChunkId chunkId, int lod, TerrainMeshData mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            if (_hasFocusChunk && _requiredChunks.Contains(chunkId))
            {
                var expectedLod = TerrainLodSelector.SelectLod(chunkId, _focusChunk);
                if (expectedLod != lod)
                    return;
            }

            var key = new PendingTerrainMeshKey(chunkId, lod);
            var collision = Math.Max(Math.Abs(chunkId.X - _focusChunk.X), Math.Abs(chunkId.Z - _focusChunk.Z)) <= _config.ColliderRadiusInChunks;

            if (_loadedChunks.TryGetValue(chunkId, out var loaded))
            {
                loaded.View.Apply(chunkId, lod, mesh, collision);
                _loadedChunks[chunkId] = new LoadedTerrainChunk(loaded.View, lod, collision);
                _pendingMeshes.Remove(key);
                return;
            }

            if (!_requiredChunks.Contains(chunkId))
            {
                _pendingMeshes[key] = mesh;
                return;
            }

            var view = _factory.Create(chunkId, _config.ChunkWorldSize);
            view.Apply(chunkId, lod, mesh, collision);
            _loadedChunks[chunkId] = new LoadedTerrainChunk(view, lod, collision);
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
            _pendingMeshKeysToRemove.Clear();
            _pendingMeshes.Clear();
            _factory.Dispose();
            DestroyOwnedObject(_root);
        }

        private void LoadOrUpdateChunk(WorldChunkId chunkId, WorldChunkId focusChunk)
        {
            var lod = TerrainLodSelector.SelectLod(chunkId, focusChunk);
            var collision = Math.Max(Math.Abs(chunkId.X - focusChunk.X), Math.Abs(chunkId.Z - focusChunk.Z)) <= _config.ColliderRadiusInChunks;

            if (_loadedChunks.TryGetValue(chunkId, out var loaded)
                && loaded.Lod == lod
                && loaded.Collision == collision)
                return;

            var key = new PendingTerrainMeshKey(chunkId, lod);
            if (_pendingMeshes.TryGetValue(key, out var pendingMesh))
            {
                if (!_loadedChunks.TryGetValue(chunkId, out loaded))
                    loaded = new LoadedTerrainChunk(_factory.Create(chunkId, _config.ChunkWorldSize), lod, collision);

                loaded.View.Apply(chunkId, lod, pendingMesh, collision);
                _loadedChunks[chunkId] = new LoadedTerrainChunk(loaded.View, lod, collision);
                _pendingMeshes.Remove(key);
                return;
            }

            if (_loadRequestsThisFrame >= _config.MaxChunkLoadsPerFrame)
                return;

            var request = _config.CreateGenerationRequest(lod);
            CW.SendEvent(new OpenWorldChunkGenerationRequested(chunkId, request, GenerationOutputMask.VisualMesh));
            _loadRequestsThisFrame++;

            if (!_loadedChunks.TryGetValue(chunkId, out loaded))
            {
                loaded = new LoadedTerrainChunk(_factory.Create(chunkId, _config.ChunkWorldSize), lod, collision);
                _loadedChunks[chunkId] = loaded;
            }
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
                RemovePendingMeshes(chunkId);
            }

            _chunksToUnload.Clear();
        }

        private void RemovePendingMeshes(WorldChunkId chunkId)
        {
            _pendingMeshKeysToRemove.Clear();
            foreach (var key in _pendingMeshes.Keys)
            {
                if (key.ChunkId == chunkId)
                    _pendingMeshKeysToRemove.Add(key);
            }

            for (var i = 0; i < _pendingMeshKeysToRemove.Count; i++)
                _pendingMeshes.Remove(_pendingMeshKeysToRemove[i]);

            _pendingMeshKeysToRemove.Clear();
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

        private readonly struct PendingTerrainMeshKey : IEquatable<PendingTerrainMeshKey>
        {
            public PendingTerrainMeshKey(WorldChunkId chunkId, int lod)
            {
                ChunkId = chunkId;
                Lod = lod;
            }

            public readonly WorldChunkId ChunkId;
            public readonly int Lod;

            public bool Equals(PendingTerrainMeshKey other)
            {
                return ChunkId == other.ChunkId && Lod == other.Lod;
            }

            public override bool Equals(object obj)
            {
                return obj is PendingTerrainMeshKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ChunkId.GetHashCode() * 397) ^ Lod;
                }
            }
        }
    }
}
