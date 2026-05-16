using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceNodeSeedSystem : ISystem
    {
        private EventReceiver<ServerWT, OpenWorldChunkLoadRequested> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<OpenWorldChunkLoadRequested>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
            var factory = SW.GetResource<OpenWorldResourceNodeFactory>();
            var deltaStore = SW.GetResource<OpenWorldResourceNodeDeltaStore>();

            foreach (var request in _requests)
            {
                var chunk = runtime.GenerationService.GenerateChunk(request.Value.ChunkId, runtime.DefaultRequest);
                SpawnChunkResourceNodes(chunk, factory, deltaStore, runtime.DefaultRequest.Bounds);
            }
        }

        private static void SpawnChunkResourceNodes(
            GeneratedChunkData chunk,
            OpenWorldResourceNodeFactory factory,
            OpenWorldResourceNodeDeltaStore deltaStore,
            WorldChunkBounds bounds)
        {
            for (var i = 0; i < chunk.ResourcePlacements.Length; i++)
            {
                var placement = chunk.ResourcePlacements[i];
                if (deltaStore.IsDepleted(placement.PlacementId))
                    continue;
                if (ResourceNodeExists(placement.PlacementId))
                    continue;

                factory.Spawn(new OpenWorldResourceNodeSpawnSpec(
                    placement.PlacementId,
                    placement.KindId,
                    placement.ChunkId,
                    placement.Position,
                    placement.YawDegrees,
                    placement.Scale,
                    OpenWorldResourceNodeRules.StartingAmount(placement.KindId)),
                    bounds);
            }
        }

        private static bool ResourceNodeExists(long placementId)
        {
            foreach (var entity in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState>>().Entities())
            {
                if (entity.Read<OpenWorldResourceNodeState>().PlacementId == placementId)
                    return true;
            }

            return false;
        }
    }
}
