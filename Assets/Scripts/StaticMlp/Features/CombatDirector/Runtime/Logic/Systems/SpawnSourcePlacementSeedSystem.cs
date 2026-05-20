using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class SpawnSourcePlacementSeedSystem : ISystem
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
            foreach (var evt in _completed)
                SeedChunkSpawnSources(evt.Value);
        }

        private static void SeedChunkSpawnSources(in OpenWorldChunkGenerationCompleted completed)
        {
            if (completed.SpawnPlacements == null)
                throw new InvalidOperationException("Open-world chunk generation completed without spawn placements array.");

            ClearChunkSpawnSources(completed.ChunkId);

            for (var i = 0; i < completed.SpawnPlacements.Length; i++)
                CreateSource(completed.ChunkId, i, completed.SpawnPlacements[i]);
        }

        private static void ClearChunkSpawnSources(WorldChunkId chunkId)
        {
            foreach (var source in SW.Query<All<SpawnSource, SpawnSourcePlacementRef>>().Entities())
            {
                if (source.Read<SpawnSourcePlacementRef>().ChunkId != chunkId)
                    continue;

                source.Destroy();
            }
        }

        private static void CreateSource(WorldChunkId chunkId, int placementIndex, in SpawnPlacement placement)
        {
            if (placement.ChunkId != chunkId)
                throw new InvalidOperationException("Open-world spawn placement chunk id must match completed chunk id.");

            var position = new float3(placement.Position.x, placement.Position.y, placement.Position.z);
            if (!math.all(math.isfinite(position)))
                throw new InvalidOperationException("Open-world spawn placement position must be finite.");

            var source = SW.NewEntity<Default>();
            source.Set(SpawnSourcePlacementRules.CreateSource(placement.KindId, position, placement.Scale));
            source.Set(new SpawnSourcePlacementRef
            {
                ChunkId = chunkId,
                PlacementIndex = placementIndex,
                KindId = placement.KindId
            });
        }
    }
}
