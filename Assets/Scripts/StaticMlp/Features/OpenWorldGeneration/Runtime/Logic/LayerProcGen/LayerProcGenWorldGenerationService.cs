using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Runevision.Common;
using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class LayerProcGenWorldGenerationService : IWorldGenerationService, IDisposable
    {
        private const int GENERATION_TIMEOUT_MS = 5000;
        private static readonly object ACTIVE_LOCK = new();
        private static LayerProcGenWorldGenerationService _activeService;

        private readonly LayerManager _manager;
        private readonly Dictionary<WorldChunkId, TopLayerDependency>[] _dependenciesByLod = new Dictionary<WorldChunkId, TopLayerDependency>[LpgTerrainMeshLayer.LOD_COUNT];
        private LayerProcGenWorldSettings _settings;
        private bool _hasSettings;
        private bool _disposed;

        public LayerProcGenWorldGenerationService()
        {
            lock (ACTIVE_LOCK)
            {
                if (_activeService != null)
                    throw new InvalidOperationException("Only one LayerProcGen world generation service can be active because LayerProcGen layers are process-wide singletons.");
                if (LayerManager.instance != null)
                    throw new InvalidOperationException("LayerProcGen already has an active LayerManager.");

                _activeService = this;
            }

            _manager = new LayerManager(false);
            for (var i = 0; i < _dependenciesByLod.Length; i++)
                _dependenciesByLod[i] = new Dictionary<WorldChunkId, TopLayerDependency>();
        }

        internal static LayerProcGenWorldContext Context { get; private set; }

        public GeneratedChunkData GenerateChunk(WorldChunkId chunkId, WorldGenerationRequest request)
        {
            ThrowIfDisposed();
            if (!request.Bounds.Contains(chunkId))
                throw new ArgumentOutOfRangeException(nameof(chunkId), chunkId, "Requested chunk is outside finite world bounds.");
            if (request.Lod < 0 || request.Lod >= LpgTerrainMeshLayer.LOD_COUNT)
                throw new ArgumentOutOfRangeException(nameof(request.Lod), request.Lod, "LayerProcGen adapter supports LOD0 through LOD3.");

            var chunkWorldSize = ResolveChunkWorldSize(request.ChunkWorldSize);
            ApplyOrValidateSettings(new LayerProcGenWorldSettings(request, chunkWorldSize));

            var layer = LpgTerrainMeshLayer.instance;
            var dependency = GetOrCreateDependency(layer, chunkId, request.Lod, chunkWorldSize);
            WaitForGeneration(dependency);
            if (!layer.TryGetMesh(chunkId, request.Lod, out var mesh))
                throw new InvalidOperationException($"LayerProcGen did not produce terrain mesh for chunk {chunkId} LOD{request.Lod}.");

            return new GeneratedChunkData(chunkId, request.Lod, mesh);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            DeactivateDependencies();
            _manager.OnDestroy();
            ResetLayerProcGenSingletons();
            Context = null;

            lock (ACTIVE_LOCK)
            {
                if (_activeService == this)
                    _activeService = null;
            }
        }

        private TopLayerDependency GetOrCreateDependency(
            LpgTerrainMeshLayer layer,
            WorldChunkId chunkId,
            int lod,
            int chunkWorldSize)
        {
            var dependencies = _dependenciesByLod[lod];
            if (dependencies.TryGetValue(chunkId, out var dependency))
                return dependency;

            var chunkCenter = new Point(
                chunkId.X * chunkWorldSize + chunkWorldSize / 2,
                chunkId.Z * chunkWorldSize + chunkWorldSize / 2);
            dependency = new TopLayerDependency(layer, new Point(chunkWorldSize, chunkWorldSize), lod);
            dependency.SetFocus(chunkCenter);
            dependencies.Add(chunkId, dependency);
            return dependency;
        }

        private void DeactivateDependencies()
        {
            for (var lod = 0; lod < _dependenciesByLod.Length; lod++)
            {
                foreach (var dependency in _dependenciesByLod[lod].Values)
                    dependency.isActive = false;
            }

            WaitForGenerationToIdle();

            for (var lod = 0; lod < _dependenciesByLod.Length; lod++)
                _dependenciesByLod[lod].Clear();
        }

        private void ApplyOrValidateSettings(LayerProcGenWorldSettings settings)
        {
            if (!_hasSettings)
            {
                _settings = settings;
                _hasSettings = true;
                Context = new LayerProcGenWorldContext(settings);
                return;
            }

            if (!_settings.Equals(settings))
                throw new InvalidOperationException("LayerProcGen world generation request settings changed after service initialization. Create a new service for different seed, bounds, chunk size, base quad count, skirts, or skirt depth.");
        }

        private static int ResolveChunkWorldSize(float chunkWorldSize)
        {
            var rounded = (int)Math.Round(chunkWorldSize);
            if (Math.Abs(chunkWorldSize - rounded) > 0.0001f)
                throw new ArgumentOutOfRangeException(nameof(chunkWorldSize), chunkWorldSize, "LayerProcGen adapter requires integer chunk world size.");

            return rounded;
        }

        private static void WaitForGeneration(TopLayerDependency dependency)
        {
            var stopwatch = Stopwatch.StartNew();
            while (dependency.changed || LayerManager.instance.building)
            {
                if (stopwatch.ElapsedMilliseconds > GENERATION_TIMEOUT_MS)
                    throw new TimeoutException("LayerProcGen terrain generation timed out.");

                Thread.Sleep(1);
            }
        }

        private static void WaitForGenerationToIdle()
        {
            var stopwatch = Stopwatch.StartNew();
            while (LayerManager.instance.building || HasPendingDependencyChanges())
            {
                if (stopwatch.ElapsedMilliseconds > GENERATION_TIMEOUT_MS)
                    throw new TimeoutException("LayerProcGen terrain generation cleanup timed out.");

                Thread.Sleep(1);
            }
        }

        private static bool HasPendingDependencyChanges()
        {
            lock (LayerManager.instance.topDependencies)
            {
                foreach (var dependency in LayerManager.instance.topDependencies)
                {
                    if (dependency.changed)
                        return true;
                }
            }

            return false;
        }

        private static void ResetLayerProcGenSingletons()
        {
            var reset = typeof(AbstractDataLayer).GetMethod("ResetInstances", BindingFlags.Static | BindingFlags.NonPublic);
            if (reset == null)
                throw new MissingMethodException(nameof(AbstractDataLayer), "ResetInstances");

            reset.Invoke(null, null);
            LayerManager.instance = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LayerProcGenWorldGenerationService));
        }
    }
}
