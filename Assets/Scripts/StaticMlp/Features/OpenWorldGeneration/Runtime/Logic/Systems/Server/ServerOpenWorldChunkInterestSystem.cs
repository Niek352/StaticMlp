using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldChunkInterestSystem : ISystem
    {
        private readonly HashSet<WorldChunkId> _desiredChunks = new();
        private readonly List<WorldChunkId> _desiredOrder = new();
        private readonly List<WorldChunkId> _loadedSnapshot = new();
        private readonly List<NetworkPeerId> _trackedPeers = new();

        public void Update()
        {
            var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
            var state = SW.GetResource<OpenWorldChunkStreamingState>();
            RemoveDisconnectedPeers(state, runtime.DefaultRequest.Bounds);

            var remainingServerLoads = runtime.MaxChunkGenerationsPerFrame;
            for (var i = 0; i < ServerPeerRegistry.Peers.Count; i++)
            {
                var peer = ServerPeerRegistry.Peers[i];
                if (!ServerPeerPlayers.TryGetPlayerPosition(peer, out var position))
                    continue;

                BuildDesiredChunks(position.x, position.z, runtime);
                UnloadChunksOutsideInterest(peer, state, runtime.DefaultRequest.Bounds);
                remainingServerLoads = LoadChunksInsideInterest(peer, state, runtime, remainingServerLoads);
            }
        }

        private void RemoveDisconnectedPeers(OpenWorldChunkStreamingState state, WorldChunkBounds bounds)
        {
            _trackedPeers.Clear();
            state.CopyTrackedPeers(_trackedPeers);

            for (var i = 0; i < _trackedPeers.Count; i++)
            {
                var peer = _trackedPeers[i];
                if (ServerPeerRegistry.Peers.Contains(peer))
                    continue;

                UnloadAllPeerChunks(peer, state, bounds);
                state.RemovePeer(peer);
            }

            _trackedPeers.Clear();
        }

        private void UnloadAllPeerChunks(NetworkPeerId peer, OpenWorldChunkStreamingState state, WorldChunkBounds bounds)
        {
            var loaded = state.LoadedChunksFor(peer);
            _loadedSnapshot.Clear();
            _loadedSnapshot.AddRange(loaded);

            for (var i = 0; i < _loadedSnapshot.Count; i++)
            {
                var chunkId = _loadedSnapshot[i];
                loaded.Remove(chunkId);
                UnloadServerChunkIfUnused(chunkId, bounds, state);
            }

            _loadedSnapshot.Clear();
        }

        private void UnloadChunksOutsideInterest(NetworkPeerId peer, OpenWorldChunkStreamingState state, WorldChunkBounds bounds)
        {
            var loaded = state.LoadedChunksFor(peer);
            _loadedSnapshot.Clear();
            _loadedSnapshot.AddRange(loaded);

            for (var i = 0; i < _loadedSnapshot.Count; i++)
            {
                var chunkId = _loadedSnapshot[i];
                if (_desiredChunks.Contains(chunkId))
                    continue;

                var clusterId = OpenWorldSpatialClusterIds.ToClusterId(chunkId, bounds);
                SW.SendToPeer(peer, new OpenWorldChunkUnloadEvent(chunkId, clusterId));
                loaded.Remove(chunkId);
                UnloadServerChunkIfUnused(chunkId, bounds, state);
            }

            _loadedSnapshot.Clear();
        }

        private int LoadChunksInsideInterest(
            NetworkPeerId peer,
            OpenWorldChunkStreamingState state,
            OpenWorldGenerationServerRuntime runtime,
            int remainingServerLoads)
        {
            var loaded = state.LoadedChunksFor(peer);
            for (var i = 0; i < _desiredOrder.Count; i++)
            {
                var chunkId = _desiredOrder[i];
                if (loaded.Contains(chunkId))
                    continue;

                var serverLoaded = state.ServerHasLoadedChunk(chunkId);
                var serverLoading = state.ServerIsLoadingChunk(chunkId);
                var requiresServerLoad = !serverLoaded && !serverLoading;
                if (requiresServerLoad && remainingServerLoads <= 0)
                    continue;

                var clusterId = OpenWorldSpatialClusterIds.ToClusterId(chunkId, runtime.DefaultRequest.Bounds);
                if (requiresServerLoad)
                {
                    ActivateServerChunk(chunkId, clusterId, state);
                    remainingServerLoads--;
                }

                loaded.Add(chunkId);

                if (serverLoaded)
                    state.QueueSnapshot(peer, chunkId, clusterId);
            }

            return remainingServerLoads;
        }

        private static void ActivateServerChunk(WorldChunkId chunkId, ushort clusterId, OpenWorldChunkStreamingState state)
        {
            if (state.ServerHasLoadedChunk(chunkId) || state.ServerIsLoadingChunk(chunkId))
                return;

            if (!SW.ClusterIsRegistered(clusterId))
                SW.RegisterCluster(clusterId);

            SW.SetActiveCluster(clusterId, true);
            if (state.TryGetSnapshot(chunkId, out var snapshot))
            {
                //SW.Serializer.LoadClusterSnapshot(snapshot);
            }
            else
            {
                SW.SendEvent(new OpenWorldChunkLoadRequested(chunkId, clusterId));
            }

            state.MarkServerLoading(chunkId);
        }

        private static void UnloadServerChunkIfUnused(
            WorldChunkId chunkId,
            WorldChunkBounds bounds,
            OpenWorldChunkStreamingState state)
        {
            if (state.AnyPeerHasChunk(chunkId))
                return;

            var clusterId = OpenWorldSpatialClusterIds.ToClusterId(chunkId, bounds);
            SW.GetResource<OpenWorldServerChunkGeometryRuntime>().Remove(chunkId);
            SW.GetResource<OpenWorldNavMeshSurfaceRuntime>().Remove(chunkId);
            state.SetSnapshot(chunkId, null);
            SW.SetActiveCluster(clusterId, false);
            //TODO:Rework in future
            /*state.SetSnapshot(
                chunkId,
                SW.Serializer.CreateClusterSnapshot(
                    clusterId,
                    withCustomSnapshotData: false,
                    gzip: true,
                    strategy: ChunkWritingStrategy.All,
                    withEntitiesData: true));
            ReadOnlySpan<ushort> clusters = stackalloc ushort[] { clusterId };
            SW.Query().BatchUnload(EntityStatusType.Any, clusters: clusters);
            state.MarkServerUnloaded(chunkId);*/
        }

        private void BuildDesiredChunks(float worldX, float worldZ, OpenWorldGenerationServerRuntime runtime)
        {
            _desiredChunks.Clear();
            _desiredOrder.Clear();

            var bounds = runtime.DefaultRequest.Bounds;
            var focus = bounds.Clamp(WorldChunkId.FromWorldPosition(worldX, worldZ, runtime.DefaultRequest.ChunkWorldSize));
            var radius = runtime.StaticStreamingRadiusInChunks;
            var minX = Math.Max(focus.X - radius, bounds.MinX);
            var maxX = Math.Min(focus.X + radius, bounds.MaxX);
            var minZ = Math.Max(focus.Z - radius, bounds.MinZ);
            var maxZ = Math.Min(focus.Z + radius, bounds.MaxZ);

            for (var z = minZ; z <= maxZ; z++)
            for (var x = minX; x <= maxX; x++)
            {
                var chunkId = new WorldChunkId(x, z);
                _desiredChunks.Add(chunkId);
                _desiredOrder.Add(chunkId);
            }
        }
    }
}
