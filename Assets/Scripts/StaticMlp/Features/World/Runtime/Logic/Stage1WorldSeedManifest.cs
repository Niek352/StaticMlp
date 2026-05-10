using System;
using UnityEngine;

namespace StaticMlp.Features.World
{
    public static class Stage1WorldSeedManifest
    {
        private const ushort MONSTER_BEHAVIOR_ID = 1;
        private const ushort PEACEFUL_BUILDER_BEHAVIOR_ID = 2;

        private static readonly Stage1BotSpawnSeed[] INITIAL_BOT_SPAWNS =
        {
            new() { Position = new Vector3(0f, 0f, 6f), BehaviorId = MONSTER_BEHAVIOR_ID, Health01 = 1f, Hunger = 0.05f, Fear = 0.1f, LeaderIndex = -1 },
            new() { Position = new Vector3(0f, 0f, 8f), BehaviorId = PEACEFUL_BUILDER_BEHAVIOR_ID, Health01 = 1f, Hunger = 0.1f, Fear = 0.05f, LeaderIndex = -1 },
            new() { Position = new Vector3(-2.5f, 0f, 10f), BehaviorId = PEACEFUL_BUILDER_BEHAVIOR_ID, Health01 = 1f, Hunger = 0.15f, Fear = 0.05f, LeaderIndex = 0 },
            new() { Position = new Vector3(2.5f, 0f, 10f), BehaviorId = PEACEFUL_BUILDER_BEHAVIOR_ID, Health01 = 0.35f, Hunger = 0.1f, Fear = 0.55f, LeaderIndex = 0 }
        };

        private static readonly RegionId[] STARTING_UNLOCKED_REGIONS =
        {
            RegionCatalog.HomeCampId
        };

        private static readonly ExpeditionId[] STARTING_UNLOCKED_EXPEDITIONS = Array.Empty<ExpeditionId>();
        private static readonly RaidId[] STARTING_UNLOCKED_RAIDS = Array.Empty<RaidId>();

        public static Stage1WorldSeed CreateResource()
        {
            return new Stage1WorldSeed(
                INITIAL_BOT_SPAWNS,
                STARTING_UNLOCKED_REGIONS,
                STARTING_UNLOCKED_EXPEDITIONS,
                STARTING_UNLOCKED_RAIDS);
        }
    }
}
