using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Features.World;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiBotSeedSystem : ISystem
    {
        private bool _spawned;

        public void Update()
        {
            if (_spawned || HasAnyBot())
            {
                _spawned = true;
                return;
            }

            var resource = SW.GetResource<Stage1WorldSeed>();
            var definitions = resource.InitialBotSpawns;
            var spawnedBots = new EntityGID[definitions.Length];

            for (var i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                var leader = ResolveLeader(definition.LeaderIndex, spawnedBots);
                var behaviorId = definition.BehaviorId == 0
                    ? CombatEnemyBehaviorIds.Default
                    : definition.BehaviorId;
                spawnedBots[i] = AiBotSpawns.SpawnBot(
                    definition.Position,
                    leader,
                    behaviorId,
                    definition.Health01,
                    definition.Hunger,
                    definition.Fear);
            }

            _spawned = true;
        }

        private static EntityGID ResolveLeader(int leaderIndex, EntityGID[] spawnedBots)
        {
            return leaderIndex >= 0 && leaderIndex < spawnedBots.Length
                ? spawnedBots[leaderIndex]
                : default;
        }

        private static bool HasAnyBot()
        {
            foreach (var _ in SW.Query<All<AiAgentTag>>().Entities())
                return true;

            return false;
        }
    }
}
