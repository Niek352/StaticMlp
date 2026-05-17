using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceNodeSeedSystem : ISystem
    {
        private EventReceiver<ServerWT, OpenWorldChunkGenerationCompleted> _completed;

        public void Init()
        {
            _completed = SW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _completed);
        }

        public void Update()
        {
            var factory = SW.GetResource<OpenWorldResourceNodeFactory>();
            var deltaStore = SW.GetResource<OpenWorldResourceNodeDeltaStore>();
            var bounds = SW.GetResource<OpenWorldGenerationServerRuntime>().DefaultRequest.Bounds;

            foreach (var evt in _completed)
            {
                if (evt.Value.ResourcePlacements.Length == 0)
                    continue;

                SpawnChunkResourceNodes(evt.Value, factory, deltaStore, bounds);
            }
        }

        private static void SpawnChunkResourceNodes(
            OpenWorldChunkGenerationCompleted chunk,
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
