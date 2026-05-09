using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

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

            var resource = SW.GetResource<InitialBotSpawningResource>();
            var definitions = resource.Spawns;
            var spawnedBots = new EntityGID[definitions.Length];

            for (var i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                var leader = ResolveLeader(definition.LeaderIndex, spawnedBots);
                var behaviorId = definition.BehaviorId == 0
                    ? AiBehaviorIds.Monster
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
