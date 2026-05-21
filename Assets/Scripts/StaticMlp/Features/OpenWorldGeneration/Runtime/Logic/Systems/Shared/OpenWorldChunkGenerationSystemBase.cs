using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.LayerProcLite;
using Unity.Mathematics;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public abstract class OpenWorldChunkGenerationSystemBase<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private EventReceiver<TWorld, OpenWorldChunkGenerationRequested> _requests;
        private OpenWorldChunkGenerationRuntime _runtime;
        private Dictionary<OpenWorldGenerationRequestKey, PendingChunkGeneration> _pending;
        private List<OpenWorldGenerationRequestKey> _keysToRemove;
        private List<OpenWorldChunkGenerationCompleted> _completed;

        protected abstract EventReceiver<TWorld, OpenWorldChunkGenerationRequested> RegisterReceiver();
        protected abstract void DeleteReceiver(ref EventReceiver<TWorld, OpenWorldChunkGenerationRequested> receiver);
        protected abstract OpenWorldChunkGenerationRuntime GetRuntime();
        protected abstract void SendCompleted(in OpenWorldChunkGenerationCompleted evt);

        public void Init()
        {
            _requests = RegisterReceiver();
            _runtime = GetRuntime();
            _pending = new Dictionary<OpenWorldGenerationRequestKey, PendingChunkGeneration>(64);
            _keysToRemove = new List<OpenWorldGenerationRequestKey>(16);
            _completed = new List<OpenWorldChunkGenerationCompleted>(16);
        }

        public void Update()
        {
            ProcessNewRequests();
            _runtime.LayerRuntime.Tick();
            ProcessPendingGenerations();
            EmitCompletedEvents();
        }

        public void Destroy()
        {
            DeleteReceiver(ref _requests);

            foreach (var pending in _pending.Values)
                ReleaseTopDependencies(pending);

            _pending.Clear();
            _keysToRemove.Clear();
            _completed.Clear();
        }

        private void ProcessNewRequests()
        {
            foreach (var request in _requests)
            {
                ValidateRequest(request.Value);
                var key = CreateRequestKey(request.Value, _runtime.WaterLevel);
                if (_pending.ContainsKey(key))
                    continue;

                _pending.Add(key, CreatePendingGeneration(request.Value));
            }
        }

        private void ProcessPendingGenerations()
        {
            _keysToRemove.Clear();

            foreach (var kvp in _pending)
            {
                var pending = kvp.Value;
                if (!AllTopDependenciesReady(pending))
                    continue;

                _completed.Add(BuildCompletedEvent(pending));
                _keysToRemove.Add(kvp.Key);
            }

            for (var i = 0; i < _keysToRemove.Count; i++)
            {
                var key = _keysToRemove[i];
                var pending = _pending[key];
                ReleaseTopDependencies(pending);
                _pending.Remove(key);
            }

            _keysToRemove.Clear();
        }

        private void EmitCompletedEvents()
        {
            for (var i = 0; i < _completed.Count; i++)
            {
                var evt = _completed[i];
                SendCompleted(in evt);
            }

            _completed.Clear();
        }

        private PendingChunkGeneration CreatePendingGeneration(in OpenWorldChunkGenerationRequested request)
        {
            var outputLayers = OpenWorldGenerationLayerCatalog.ToOutputLayerIds(request.Outputs);
            var topDependencies = new LayerProcLiteTopDependencyId[outputLayers.Length];
            var bounds = ToLayerBounds(request.ChunkId, request.Request.ChunkWorldSize);
            var settingsHash = ComputeSettingsHash(request.Request, _runtime.WaterLevel);
            var settings = new OpenWorldLayerGenerationSettings(
                (uint)request.Request.Seed.Value,
                request.Request.ChunkWorldSize,
                _runtime.WaterLevel,
                request.Request.BaseQuadCount,
                request.Request.Lod,
                request.Request.AddSkirts,
                request.Request.SkirtDepth);

            for (var i = 0; i < outputLayers.Length; i++)
            {
                topDependencies[i] = _runtime.LayerRuntime.AddTopDependency(
                    new LayerProcLiteTopDependencyRequest(
                        outputLayers[i],
                        0,
                        bounds,
                        request.Request.Lod,
                        settingsHash,
                        settings));
            }

            return new PendingChunkGeneration(
                request.ChunkId,
                request.Request.ChunkWorldSize,
                request.Request.Lod,
                request.Outputs,
                outputLayers,
                topDependencies);
        }

        private bool AllTopDependenciesReady(PendingChunkGeneration pending)
        {
            for (var i = 0; i < pending.TopDependencies.Length; i++)
            {
                if (!_runtime.LayerRuntime.IsTopDependencyReady(pending.TopDependencies[i]))
                    return false;
            }

            return true;
        }

        private void ReleaseTopDependencies(PendingChunkGeneration pending)
        {
            for (var i = 0; i < pending.TopDependencies.Length; i++)
                _runtime.LayerRuntime.RemoveTopDependency(pending.TopDependencies[i]);
        }

        private OpenWorldChunkGenerationCompleted BuildCompletedEvent(PendingChunkGeneration pending)
        {
            TerrainMeshData mesh = null;
            TerrainMeshData navMeshSourceMesh = null;
            ResourcePlacement[] resourcePlacements = Array.Empty<ResourcePlacement>();
            SpawnPlacement[] spawnPlacements = Array.Empty<SpawnPlacement>();

            var meshData = HasMeshOutput(pending.Outputs)
                ? GetLayerData<OpenWorldMeshChunkData>(pending, OpenWorldGenerationLayerIds.MeshData)
                : null;

            if ((pending.Outputs & (GenerationOutputMask.VisualMesh | GenerationOutputMask.PhysicsMesh)) != 0)
                mesh = BuildManagedMesh(meshData);

            if (pending.Outputs.HasFlag(GenerationOutputMask.NavMeshSourceMesh))
                navMeshSourceMesh = BuildManagedNavMeshSourceMesh(meshData);

            if (pending.Outputs.HasFlag(GenerationOutputMask.Placements))
            {
                var placementData = GetLayerData<OpenWorldPlacementChunkData>(
                    pending,
                    OpenWorldGenerationLayerIds.Placements);
                var resourceCount = placementData.ResourcePlacementCount[0];
                resourcePlacements = new ResourcePlacement[resourceCount];
                for (var i = 0; i < resourceCount; i++)
                    resourcePlacements[i] = placementData.ResourcePlacements[i];

                var spawnCount = placementData.SpawnPlacementCount[0];
                spawnPlacements = new SpawnPlacement[spawnCount];
                for (var i = 0; i < spawnCount; i++)
                    spawnPlacements[i] = placementData.SpawnPlacements[i];
            }

            return new OpenWorldChunkGenerationCompleted(
                pending.ChunkId,
                pending.ChunkWorldSize,
                pending.Lod,
                pending.Outputs,
                pending.Outputs.HasFlag(GenerationOutputMask.VisualMesh) ? mesh : null,
                pending.Outputs.HasFlag(GenerationOutputMask.PhysicsMesh) ? mesh : null,
                navMeshSourceMesh,
                resourcePlacements,
                spawnPlacements);
        }

        private TData GetLayerData<TData>(PendingChunkGeneration pending, LayerProcLiteLayerId layerId)
            where TData : class, ILayerProcLiteChunkData
        {
            for (var i = 0; i < pending.OutputLayers.Length; i++)
            {
                if (pending.OutputLayers[i] != layerId)
                    continue;

                var rootKey = _runtime.LayerRuntime.GetSingleRootKey(pending.TopDependencies[i]);
                return _runtime.LayerRuntime.GetChunkData<TData>(rootKey);
            }

            throw new InvalidOperationException($"Pending generation does not contain output layer {layerId.Value}.");
        }

        private static TerrainMeshData BuildManagedMesh(OpenWorldMeshChunkData meshData)
        {
            var vertices = new Vector3[meshData.Vertices.Length];
            for (var i = 0; i < meshData.Vertices.Length; i++)
            {
                var v = meshData.Vertices[i];
                vertices[i] = new Vector3(v.x, v.y, v.z);
            }

            var normals = new Vector3[meshData.Normals.Length];
            for (var i = 0; i < meshData.Normals.Length; i++)
            {
                var n = meshData.Normals[i];
                normals[i] = new Vector3(n.x, n.y, n.z);
            }

            var tangents = new Vector4[meshData.Tangents.Length];
            for (var i = 0; i < meshData.Tangents.Length; i++)
            {
                var t = meshData.Tangents[i];
                tangents[i] = new Vector4(t.x, t.y, t.z, t.w);
            }

            var uvs = new Vector2[meshData.Uvs.Length];
            for (var i = 0; i < meshData.Uvs.Length; i++)
            {
                var u = meshData.Uvs[i];
                uvs[i] = new Vector2(u.x, u.y);
            }

            var colors = new Color32[meshData.ColorsRgba.Length];
            for (var i = 0; i < meshData.ColorsRgba.Length; i++)
                colors[i] = UnpackRgba(meshData.ColorsRgba[i]);

            var triangles = new int[meshData.Triangles.Length];
            meshData.Triangles.CopyTo(triangles);

            var minY = meshData.OutMinY[0];
            var maxY = meshData.OutMaxY[0];
            var boundsHeight = maxY - minY;
            var bounds = new Bounds(
                new Vector3(meshData.ChunkWorldSize * 0.5f, minY + boundsHeight * 0.5f, meshData.ChunkWorldSize * 0.5f),
                new Vector3(meshData.ChunkWorldSize, boundsHeight, meshData.ChunkWorldSize));

            return new TerrainMeshData(vertices, normals, tangents, uvs, colors, triangles, bounds);
        }

        private static TerrainMeshData BuildManagedNavMeshSourceMesh(OpenWorldMeshChunkData meshData)
        {
            var vertices = new Vector3[meshData.Vertices.Length];
            for (var i = 0; i < meshData.Vertices.Length; i++)
            {
                var v = meshData.Vertices[i];
                vertices[i] = new Vector3(v.x, v.y, v.z);
            }

            var triangles = new int[meshData.Triangles.Length];
            meshData.Triangles.CopyTo(triangles);

            var minY = meshData.OutMinY[0];
            var maxY = meshData.OutMaxY[0];
            var boundsHeight = maxY - minY;
            var bounds = new Bounds(
                new Vector3(meshData.ChunkWorldSize * 0.5f, minY + boundsHeight * 0.5f, meshData.ChunkWorldSize * 0.5f),
                new Vector3(meshData.ChunkWorldSize, boundsHeight, meshData.ChunkWorldSize));

            return new TerrainMeshData(vertices, null, null, null, null, triangles, bounds);
        }

        private void ValidateRequest(in OpenWorldChunkGenerationRequested request)
        {
            if (request.Outputs == GenerationOutputMask.None)
                throw new ArgumentOutOfRangeException(nameof(request.Outputs), request.Outputs, "Open world generation request must include at least one output layer.");
            if (!_runtime.Bounds.Equals(request.Request.Bounds))
                throw new InvalidOperationException("Open world generation request bounds do not match the registered generation runtime.");
            if (_runtime.Seed != request.Request.Seed)
                throw new InvalidOperationException("Open world generation request seed does not match the registered generation runtime.");
            if (!Mathf.Approximately(_runtime.ChunkWorldSize, request.Request.ChunkWorldSize))
                throw new InvalidOperationException("Open world generation request chunk size does not match the registered generation runtime.");
            if (_runtime.BaseQuadCount != request.Request.BaseQuadCount)
                throw new InvalidOperationException("Open world generation request base quad count does not match the registered generation runtime.");
            if (!request.Request.Bounds.Contains(request.ChunkId))
                throw new ArgumentOutOfRangeException(nameof(request.ChunkId), request.ChunkId, "Requested chunk is outside generation bounds.");
        }

        private static OpenWorldGenerationRequestKey CreateRequestKey(in OpenWorldChunkGenerationRequested request, float waterLevel)
        {
            return new OpenWorldGenerationRequestKey(
                new LayerProcLiteChunkId(request.ChunkId.X, request.ChunkId.Z),
                request.Request.Lod,
                OpenWorldGenerationLayerCatalog.ToLayerMask(request.Outputs),
                request.Outputs,
                ComputeSettingsHash(request.Request, waterLevel));
        }

        private static uint ComputeSettingsHash(WorldGenerationRequest request, float waterLevel)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = Mix(hash, request.Seed.Value);
                hash = Mix(hash, request.Bounds.MinX);
                hash = Mix(hash, request.Bounds.MaxX);
                hash = Mix(hash, request.Bounds.MinZ);
                hash = Mix(hash, request.Bounds.MaxZ);
                hash = Mix(hash, math.asuint(request.ChunkWorldSize));
                hash = Mix(hash, request.BaseQuadCount);
                hash = Mix(hash, request.Lod);
                hash = Mix(hash, request.AddSkirts ? 1 : 0);
                hash = Mix(hash, math.asuint(request.SkirtDepth));
                hash = Mix(hash, math.asuint(waterLevel));
                return hash;
            }
        }

        private static uint Mix(uint hash, int value)
        {
            return Mix(hash, (uint)value);
        }

        private static uint Mix(uint hash, uint value)
        {
            unchecked
            {
                hash ^= value;
                hash *= 16777619u;
                return hash;
            }
        }

        private static bool HasMeshOutput(GenerationOutputMask outputs)
        {
            return (outputs & (GenerationOutputMask.VisualMesh | GenerationOutputMask.PhysicsMesh | GenerationOutputMask.NavMeshSourceMesh)) != 0;
        }

        private static LayerProcLiteWorldBounds ToLayerBounds(WorldChunkId chunkId, float chunkWorldSize)
        {
            var minX = chunkId.X * chunkWorldSize;
            var minZ = chunkId.Z * chunkWorldSize;
            return new LayerProcLiteWorldBounds(minX, minZ, minX + chunkWorldSize, minZ + chunkWorldSize);
        }

        private static Color32 UnpackRgba(uint rgba)
        {
            return new Color32(
                (byte)(rgba >> 24),
                (byte)(rgba >> 16),
                (byte)(rgba >> 8),
                (byte)rgba);
        }

        private readonly struct PendingChunkGeneration
        {
            public PendingChunkGeneration(
                WorldChunkId chunkId,
                float chunkWorldSize,
                int lod,
                GenerationOutputMask outputs,
                LayerProcLiteLayerId[] outputLayers,
                LayerProcLiteTopDependencyId[] topDependencies)
            {
                ChunkId = chunkId;
                ChunkWorldSize = chunkWorldSize;
                Lod = lod;
                Outputs = outputs;
                OutputLayers = outputLayers;
                TopDependencies = topDependencies;
            }

            public readonly WorldChunkId ChunkId;
            public readonly float ChunkWorldSize;
            public readonly int Lod;
            public readonly GenerationOutputMask Outputs;
            public readonly LayerProcLiteLayerId[] OutputLayers;
            public readonly LayerProcLiteTopDependencyId[] TopDependencies;
        }
    }
}
