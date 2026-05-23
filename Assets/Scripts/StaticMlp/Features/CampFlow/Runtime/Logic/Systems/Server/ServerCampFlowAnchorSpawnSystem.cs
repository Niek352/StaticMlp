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

namespace StaticMlp.Features.CampFlow
{
    public sealed class ServerCampFlowAnchorSpawnSystem : ISystem
    {
        private bool _spawned;

        public void Update()
        {
            if (_spawned || HasAnyCampAnchor())
            {
                _spawned = true;
                return;
            }

            var settlementSeed = SW.GetResource<SettlementSeed>();
            var progressionSeed = SW.GetResource<ProgressionSeed>();
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
            SettlementConstructionSiteSeed siteSeed,
            ProgressionSeed progressionSeed,
            IHeightSampler heightSampler)
        {
            var position = siteSeed.Position;
            position.y = heightSampler.SampleHeight(position.x, position.z);

            SW.GetResource<CampFlowAnchorFactory>().Spawn(new CampFlowAnchorSpawnSpec(
                new SettlementAnchorId(siteSeed.AnchorId),
                position,
                siteSeed.Rotation,
                BuildFlagMask(progressionSeed)));
        }

        private static uint BuildFlagMask(ProgressionSeed seed)
        {
            var mask = 0u;
            for (var i = 0; i < seed.StartingFlags.Length; i++)
                mask |= ProgressionState.GetFlagBit(seed.StartingFlags[i]);

            return mask;
        }

        private static bool HasAnyCampAnchor()
        {
            foreach (var _ in SW.Query<All<CampFlowProgression>>().Entities())
                return true;

            return false;
        }
    }
}
