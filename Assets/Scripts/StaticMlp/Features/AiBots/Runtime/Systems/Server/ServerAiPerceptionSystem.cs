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
        private const float DETECTION_DISTANCE = 25f;

        public void Update()
        {
            foreach (var bot in SW.Query<All<ServerOwned, AiAgentTag, SW.Multi<AiBlackboardEntry>, CharacterNetState>>().Entities())
            {
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

                if (nearestDistance <= DETECTION_DISTANCE)
                {
                    AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, nearestEnemy);
                    AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.EnemyDistance, nearestDistance);
                    AiBlackboardAccess.SetVector(bot, AiCoreVariableIds.LastKnownEnemyPosition, nearestPosition);
                    AiBlackboardAccess.SetFloat(
                        bot,
                        AiCoreVariableIds.Fear,
                        MathF.Min(1f, AiBlackboardAccess.GetFloat(bot, AiCoreVariableIds.Fear) + Time.deltaTime * 0.35f));
                    continue;
                }

                AiBlackboardAccess.Remove(bot, AiCoreVariableIds.Enemy);
                AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.EnemyDistance, 999f);
            }
        }
    }
}
