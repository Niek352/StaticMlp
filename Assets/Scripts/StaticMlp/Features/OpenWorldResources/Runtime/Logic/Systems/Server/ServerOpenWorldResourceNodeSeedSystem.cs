using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceNodeSeedSystem : ISystem
    {
        private WorldChunkBounds _bounds;
        private int _nextX;
        private int _nextZ;
        private bool _complete;

        public void Init()
        {
            _bounds = SW.GetResource<OpenWorldGenerationServerRuntime>().DefaultRequest.Bounds;
            _nextX = _bounds.MinX;
            _nextZ = _bounds.MinZ;
        }

        public void Update()
        {
            if (_complete)
                return;

            var runtime = SW.GetResource<OpenWorldGenerationServerRuntime>();
            var factory = SW.GetResource<OpenWorldResourceNodeFactory>();
            var deltaStore = SW.GetResource<OpenWorldResourceNodeDeltaStore>();

            for (var i = 0; i < runtime.MaxChunkGenerationsPerFrame && !_complete; i++)
            {
                var chunkId = new WorldChunkId(_nextX, _nextZ);
                var chunk = runtime.GenerationService.GenerateChunk(chunkId, runtime.DefaultRequest);
                SpawnChunkResourceNodes(chunk, factory, deltaStore);
                AdvanceChunkCursor();
            }
        }

        private static void SpawnChunkResourceNodes(
            GeneratedChunkData chunk,
            OpenWorldResourceNodeFactory factory,
            OpenWorldResourceNodeDeltaStore deltaStore)
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
                    OpenWorldResourceNodeRules.StartingAmount(placement.KindId)));
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

        private void AdvanceChunkCursor()
        {
            if (_nextX < _bounds.MaxX)
            {
                _nextX++;
                return;
            }

            _nextX = _bounds.MinX;
            if (_nextZ < _bounds.MaxZ)
            {
                _nextZ++;
                return;
            }

            _complete = true;
        }
    }
}
