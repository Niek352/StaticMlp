using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Stage1
{
    public sealed class ServerStage1CampAnchorSpawnSystem : ISystem
    {
        private bool _spawned;

        public void Update()
        {
            if (_spawned || HasAnyCampAnchor())
            {
                _spawned = true;
                return;
            }

            var settlementSeed = SW.GetResource<Stage1SettlementSeed>();
            var progressionSeed = SW.GetResource<Stage1ProgressionSeed>();
            var heightSampler = SW.GetResource<IHeightSampler>();
            var sites = settlementSeed.InitialConstructionSites;
            var spawnedAnchorIds = new HashSet<ushort>();
            for (var i = 0; i < sites.Length; i++)
            {
                if (sites[i].AnchorId == 0)
                    continue;

                if (!spawnedAnchorIds.Add(sites[i].AnchorId))
                    throw new InvalidOperationException(
                        $"Stage 1 seed defines duplicate camp anchor {sites[i].AnchorId}.");

                SpawnAnchor(sites[i], progressionSeed, heightSampler);
            }

            _spawned = true;
        }

        private static void SpawnAnchor(
            Stage1ConstructionSiteSeed siteSeed,
            Stage1ProgressionSeed progressionSeed,
            IHeightSampler heightSampler)
        {
            var position = siteSeed.Position;
            position.y = heightSampler.SampleHeight(position.x, position.z);

            SW.GetResource<Stage1CampAnchorFactory>().Spawn(new Stage1CampAnchorSpawnSpec(
                new SettlementAnchorId(siteSeed.AnchorId),
                position,
                siteSeed.Rotation,
                BuildFlagMask(progressionSeed)));
        }

        private static uint BuildFlagMask(Stage1ProgressionSeed seed)
        {
            var mask = 0u;
            for (var i = 0; i < seed.StartingFlags.Length; i++)
                mask |= Stage1ProgressionState.GetFlagBit(seed.StartingFlags[i]);

            return mask;
        }

        private static bool HasAnyCampAnchor()
        {
            foreach (var _ in SW.Query<All<Stage1SettlementProgression>>().Entities())
                return true;

            return false;
        }
    }
}
