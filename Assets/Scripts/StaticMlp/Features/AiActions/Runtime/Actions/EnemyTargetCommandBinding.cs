using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public static class EnemyTargetCommandBinding
    {
        public static bool TryBind(SW.Entity bot, EntityGID target)
        {
            if (!target.TryUnpack<ServerWT>(out var targetEntity) || !targetEntity.Has<CharacterNetState>())
                return false;

            ref readonly var targetState = ref targetEntity.Read<CharacterNetState>();
            ref readonly var botState = ref bot.Read<CharacterNetState>();
            AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, target);
            AiBlackboardAccess.SetVector(bot, AiCoreVariableIds.LastKnownEnemyPosition, targetState.Position);
            AiBlackboardAccess.SetFloat(
                bot,
                AiCoreVariableIds.EnemyDistance,
                (targetState.Position - botState.Position).magnitude);
            return true;
        }
    }
}
