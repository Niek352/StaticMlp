using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiPerceptionSystem : ISystem
    {
        private const float DetectionDistance = 25f;

        public void Update()
        {
            foreach (var bot in SW.Query<All<ServerOwned, AiAgentTag, AiBlackboard, CharacterNetState>>().Entities())
            {
                ref var blackboard = ref bot.Mut<AiBlackboard>();
                ref readonly var botState = ref bot.Read<CharacterNetState>();

                EntityGID nearestEnemy = default;
                var nearestDistance = float.MaxValue;
                var nearestPosition = botState.Position;

                foreach (var player in SW.Query<All<PlayerTag, CharacterNetState>>().Entities())
                {
                    ref readonly var playerState = ref player.Read<CharacterNetState>();
                    var distance = Vector3.Distance(botState.Position, playerState.Position);
                    if (distance >= nearestDistance)
                        continue;

                    nearestDistance = distance;
                    nearestEnemy = player.GID;
                    nearestPosition = playerState.Position;
                }

                if (nearestDistance <= DetectionDistance)
                {
                    blackboard.Enemy = nearestEnemy;
                    blackboard.EnemyDistance = nearestDistance;
                    blackboard.LastKnownEnemyPosition = nearestPosition;
                    blackboard.Fear = MathF.Min(1f, blackboard.Fear + Time.deltaTime * 0.35f);
                    continue;
                }

                blackboard.Enemy = default;
                blackboard.EnemyDistance = 999f;
            }
        }
    }
}
