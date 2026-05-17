using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;

namespace StaticMlp.LayerProcLite
{
    public sealed class LayerProcLiteRuntime : IDisposable
    {
        private readonly Dictionary<LayerProcLiteLayerId, LayerProcLiteLayerDefinition> _layers = new();
        private readonly Dictionary<LayerProcLiteChunkKey, ChunkEntry> _chunks = new();
        private readonly Dictionary<LayerProcLiteTopDependencyId, TopDependencyEntry> _topDependencies = new();
        private readonly List<LayerProcLiteChunkKey> _releaseKeys = new();
        private int _nextTopDependencyId = 1;
        private bool _graphValidated;

        public int ActiveTopDependencyCount => _topDependencies.Count;
        public int ChunkCount => _chunks.Count;

        public int ReadyChunkCount
        {
            get
            {
                var count = 0;
                foreach (var entry in _chunks.Values)
                {
                    if (entry.State == LayerProcLiteChunkState.Ready)
                        count++;
                }

                return count;
            }
        }

        public LayerProcLiteRuntime RegisterLayer(LayerProcLiteLayerDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (!_layers.TryAdd(definition.LayerId, definition))
                throw new InvalidOperationException($"Layer {definition.LayerId.Value} is already registered.");

            _graphValidated = false;
            return this;
        }

        public LayerProcLiteTopDependencyId AddTopDependency(in LayerProcLiteTopDependencyRequest request)
        {
            var id = new LayerProcLiteTopDependencyId(_nextTopDependencyId++);
            SetTopDependency(id, request);
            return id;
        }

        public void SetTopDependency(LayerProcLiteTopDependencyId id, in LayerProcLiteTopDependencyRequest request)
        {
            ValidateGraph();
            ValidateRequest(request);

            var requiredKeys = new HashSet<LayerProcLiteChunkKey>();
            var rootKeys = new List<LayerProcLiteChunkKey>();
            ResolveRequest(request, requiredKeys, rootKeys);

            if (_topDependencies.TryGetValue(id, out var oldEntry))
                ReleaseTopDependencyRefs(oldEntry.RequiredKeys, requiredKeys);

            RetainTopDependencyRefs(requiredKeys, oldEntry?.RequiredKeys);
            _topDependencies[id] = new TopDependencyEntry(request, requiredKeys, rootKeys);
            ReleaseUnreferencedChunks();
        }

        public void RemoveTopDependency(LayerProcLiteTopDependencyId id)
        {
            if (!_topDependencies.TryGetValue(id, out var entry))
                throw new KeyNotFoundException($"Top dependency {id.Value} is not active.");

            foreach (var key in entry.RequiredKeys)
                _chunks[key].RefCount--;

            _topDependencies.Remove(id);
            ReleaseUnreferencedChunks();
        }

        public void Tick()
        {
            foreach (var entry in _chunks.Values)
            {
                if (entry.State != LayerProcLiteChunkState.Scheduled || !entry.Handle.IsCompleted)
                    continue;

                entry.Handle.Complete();
                entry.State = LayerProcLiteChunkState.Ready;
            }
        }

        public bool IsTopDependencyReady(LayerProcLiteTopDependencyId id)
        {
            if (!_topDependencies.TryGetValue(id, out var entry))
                throw new KeyNotFoundException($"Top dependency {id.Value} is not active.");

            for (var i = 0; i < entry.RootKeys.Count; i++)
            {
                if (_chunks[entry.RootKeys[i]].State != LayerProcLiteChunkState.Ready)
                    return false;
            }

            return true;
        }

        public LayerProcLiteChunkKey GetSingleRootKey(LayerProcLiteTopDependencyId id)
        {
            if (!_topDependencies.TryGetValue(id, out var entry))
                throw new KeyNotFoundException($"Top dependency {id.Value} is not active.");
            if (entry.RootKeys.Count != 1)
                throw new InvalidOperationException($"Top dependency {id.Value} expected one root chunk, got {entry.RootKeys.Count}.");

            return entry.RootKeys[0];
        }

        public TData GetChunkData<TData>(LayerProcLiteChunkKey key)
            where TData : class, ILayerProcLiteChunkData
        {
            if (!_chunks.TryGetValue(key, out var entry))
                throw new KeyNotFoundException($"Chunk {key.LayerId.Value}:{key.Level}:{key.ChunkId.X},{key.ChunkId.Z} is not loaded.");
            if (entry.State != LayerProcLiteChunkState.Ready)
                throw new InvalidOperationException($"Chunk {key.LayerId.Value}:{key.Level}:{key.ChunkId.X},{key.ChunkId.Z} is not ready.");
            if (entry.Data is not TData data)
                throw new InvalidOperationException($"Chunk data is not {typeof(TData).Name}.");

            return data;
        }

        public bool ContainsChunk(LayerProcLiteChunkKey key)
        {
            return _chunks.ContainsKey(key);
        }

        public int GetRetainCount(LayerProcLiteChunkKey key)
        {
            return _chunks.TryGetValue(key, out var entry) ? entry.RefCount : 0;
        }

        public LayerProcLiteChunkState GetChunkState(LayerProcLiteChunkKey key)
        {
            return _chunks.TryGetValue(key, out var entry) ? entry.State : LayerProcLiteChunkState.Unloaded;
        }

        public void Dispose()
        {
            foreach (var entry in _chunks.Values)
            {
                entry.Handle.Complete();
                entry.Data.Dispose();
            }

            _chunks.Clear();
            _topDependencies.Clear();
            _releaseKeys.Clear();
        }

        private void ResolveRequest(
            in LayerProcLiteTopDependencyRequest request,
            HashSet<LayerProcLiteChunkKey> requiredKeys,
            List<LayerProcLiteChunkKey> rootKeys)
        {
            var layer = _layers[request.LayerId];
            var min = request.Bounds.MinChunk(layer.ChunkWorldSize);
            var max = request.Bounds.MaxChunk(layer.ChunkWorldSize);

            for (var z = min.y; z <= max.y; z++)
            {
                for (var x = min.x; x <= max.x; x++)
                {
                    var key = new LayerProcLiteChunkKey(
                        request.LayerId,
                        request.Level,
                        new LayerProcLiteChunkId(x, z),
                        request.Variant,
                        request.SettingsHash);
                    ResolveChunk(key, requiredKeys, request.Settings);
                    rootKeys.Add(key);
                }
            }
        }

        private void ResolveChunk(
            LayerProcLiteChunkKey key,
            HashSet<LayerProcLiteChunkKey> requiredKeys,
            object settings)
        {
            if (requiredKeys.Contains(key))
                return;

            var layer = _layers[key.LayerId];
            var bounds = LayerProcLiteWorldBounds.FromChunk(key.ChunkId, layer.ChunkWorldSize);
            var providerGroups = new List<LayerProcLiteProviderSet.ProviderGroup>();
            var dependencyHandle = default(JobHandle);
            var hasDependencyHandle = false;

            for (var dependencyIndex = 0; dependencyIndex < layer.Dependencies.Length; dependencyIndex++)
            {
                var dependency = layer.Dependencies[dependencyIndex];
                if (dependency.UserLevel != key.Level)
                    continue;

                var providerLayer = _layers[dependency.ProviderLayerId];
                var providerBounds = bounds.Expanded(dependency.EffectDistanceWorld);
                var providerMin = providerBounds.MinChunk(providerLayer.ChunkWorldSize);
                var providerMax = providerBounds.MaxChunk(providerLayer.ChunkWorldSize);
                var providerChunks = new List<LayerProcLiteProviderChunkInfo>();

                for (var z = providerMin.y; z <= providerMax.y; z++)
                {
                    for (var x = providerMin.x; x <= providerMax.x; x++)
                    {
                        var providerKey = new LayerProcLiteChunkKey(
                            dependency.ProviderLayerId,
                            dependency.ProviderLevel,
                            new LayerProcLiteChunkId(x, z),
                            key.Variant,
                            key.SettingsHash);
                        ResolveChunk(providerKey, requiredKeys, settings);

                        var providerEntry = _chunks[providerKey];
                        providerChunks.Add(new LayerProcLiteProviderChunkInfo(
                            providerKey,
                            providerEntry.Bounds,
                            providerEntry.Data,
                            providerEntry.Handle));
                        AddDependencyHandle(ref dependencyHandle, ref hasDependencyHandle, providerEntry.Handle);
                    }
                }

                providerGroups.Add(new LayerProcLiteProviderSet.ProviderGroup(
                    dependency.ProviderLayerId,
                    dependency.ProviderLevel,
                    providerBounds,
                    providerChunks.ToArray()));
            }

            if (!_chunks.ContainsKey(key))
            {
                var providers = providerGroups.Count == 0
                    ? LayerProcLiteProviderSet.Empty
                    : new LayerProcLiteProviderSet(providerGroups.ToArray());
                var context = new LayerProcLiteScheduleContext(key, bounds, providers, hasDependencyHandle ? dependencyHandle : default, settings);
                var result = layer.Scheduler.Schedule(context);
                _chunks.Add(key, new ChunkEntry(key, bounds, result.Data, result.Handle));
            }

            requiredKeys.Add(key);
        }

        private void ValidateRequest(in LayerProcLiteTopDependencyRequest request)
        {
            if (!_layers.TryGetValue(request.LayerId, out var layer))
                throw new KeyNotFoundException($"Layer {request.LayerId.Value} is not registered.");
            if (request.Level >= layer.LevelCount)
                throw new ArgumentOutOfRangeException(nameof(request.Level), request.Level, $"Layer {request.LayerId.Value} does not contain that level.");
        }

        private void ValidateGraph()
        {
            if (_graphValidated)
                return;

            foreach (var layer in _layers.Values)
            {
                for (var i = 0; i < layer.Dependencies.Length; i++)
                {
                    var dependency = layer.Dependencies[i];
                    if (dependency.UserLevel >= layer.LevelCount)
                        throw new InvalidOperationException($"Layer {layer.LayerId.Value} dependency uses missing user level {dependency.UserLevel}.");
                    if (!_layers.TryGetValue(dependency.ProviderLayerId, out var providerLayer))
                        throw new KeyNotFoundException($"Layer {layer.LayerId.Value} depends on missing provider layer {dependency.ProviderLayerId.Value}.");
                    if (dependency.ProviderLevel >= providerLayer.LevelCount)
                        throw new InvalidOperationException($"Layer {layer.LayerId.Value} depends on missing provider level {dependency.ProviderLevel} in layer {dependency.ProviderLayerId.Value}.");
                }
            }

            var states = new Dictionary<LayerLevelKey, VisitState>();
            foreach (var layer in _layers.Values)
            {
                for (var level = 0; level < layer.LevelCount; level++)
                    VisitLayerLevel(new LayerLevelKey(layer.LayerId, level), states);
            }

            _graphValidated = true;
        }

        private void VisitLayerLevel(LayerLevelKey key, Dictionary<LayerLevelKey, VisitState> states)
        {
            if (states.TryGetValue(key, out var state))
            {
                if (state == VisitState.Visiting)
                    throw new InvalidOperationException($"Layer dependency cycle includes layer {key.LayerId.Value} level {key.Level}.");

                return;
            }

            states.Add(key, VisitState.Visiting);
            var layer = _layers[key.LayerId];
            for (var i = 0; i < layer.Dependencies.Length; i++)
            {
                var dependency = layer.Dependencies[i];
                if (dependency.UserLevel == key.Level)
                    VisitLayerLevel(new LayerLevelKey(dependency.ProviderLayerId, dependency.ProviderLevel), states);
            }

            states[key] = VisitState.Visited;
        }

        private void RetainTopDependencyRefs(HashSet<LayerProcLiteChunkKey> newKeys, HashSet<LayerProcLiteChunkKey> oldKeys)
        {
            foreach (var key in newKeys)
            {
                if (oldKeys != null && oldKeys.Contains(key))
                    continue;

                _chunks[key].RefCount++;
            }
        }

        private void ReleaseTopDependencyRefs(HashSet<LayerProcLiteChunkKey> oldKeys, HashSet<LayerProcLiteChunkKey> newKeys)
        {
            foreach (var key in oldKeys)
            {
                if (newKeys.Contains(key))
                    continue;

                _chunks[key].RefCount--;
            }
        }

        private void ReleaseUnreferencedChunks()
        {
            _releaseKeys.Clear();
            foreach (var pair in _chunks)
            {
                if (pair.Value.RefCount <= 0)
                    _releaseKeys.Add(pair.Key);
            }

            for (var i = 0; i < _releaseKeys.Count; i++)
            {
                var key = _releaseKeys[i];
                var entry = _chunks[key];
                entry.Handle.Complete();
                entry.Data.Dispose();
                _chunks.Remove(key);
            }

            _releaseKeys.Clear();
        }

        private static void AddDependencyHandle(ref JobHandle combined, ref bool hasDependencyHandle, JobHandle dependency)
        {
            combined = hasDependencyHandle ? JobHandle.CombineDependencies(combined, dependency) : dependency;
            hasDependencyHandle = true;
        }

        private sealed class ChunkEntry
        {
            public ChunkEntry(
                LayerProcLiteChunkKey key,
                LayerProcLiteWorldBounds bounds,
                ILayerProcLiteChunkData data,
                JobHandle handle)
            {
                Key = key;
                Bounds = bounds;
                Data = data;
                Handle = handle;
                State = LayerProcLiteChunkState.Scheduled;
            }

            public readonly LayerProcLiteChunkKey Key;
            public readonly LayerProcLiteWorldBounds Bounds;
            public readonly ILayerProcLiteChunkData Data;
            public readonly JobHandle Handle;
            public LayerProcLiteChunkState State;
            public int RefCount;
        }

        private sealed class TopDependencyEntry
        {
            public TopDependencyEntry(
                LayerProcLiteTopDependencyRequest request,
                HashSet<LayerProcLiteChunkKey> requiredKeys,
                List<LayerProcLiteChunkKey> rootKeys)
            {
                Request = request;
                RequiredKeys = requiredKeys;
                RootKeys = rootKeys;
            }

            public readonly LayerProcLiteTopDependencyRequest Request;
            public readonly HashSet<LayerProcLiteChunkKey> RequiredKeys;
            public readonly List<LayerProcLiteChunkKey> RootKeys;
        }

        private readonly struct LayerLevelKey : IEquatable<LayerLevelKey>
        {
            public LayerLevelKey(LayerProcLiteLayerId layerId, int level)
            {
                LayerId = layerId;
                Level = level;
            }

            public readonly LayerProcLiteLayerId LayerId;
            public readonly int Level;

            public bool Equals(LayerLevelKey other)
            {
                return LayerId == other.LayerId && Level == other.Level;
            }

            public override bool Equals(object obj)
            {
                return obj is LayerLevelKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (LayerId.GetHashCode() * 397) ^ Level;
                }
            }
        }

        private enum VisitState
        {
            Visiting,
            Visited
        }
    }
}
