using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Features.Shared;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerValidateCombatCommandsSystem : ISystem
    {
        private readonly List<EntityGID> _requests = new();

        public void Update()
        {
            _requests.Clear();
            foreach (var request in SW.Query<All<CombatAbilityRequest>, None<CombatAbilityValidatedTag>>().Entities())
                _requests.Add(request.GID);

            var simulationTime = SW.GetResource<SimulationTime>();
            var nowTick = simulationTime.ServerTick;
            var config = SW.GetResource<CombatConfig>();
            for (var i = 0; i < _requests.Count; i++)
            {
                if (!_requests[i].TryUnpack<ServerWT>(out var request))
                    continue;

                Validate(request, config, simulationTime, nowTick);
            }
        }

        private static void Validate(SW.Entity request, CombatConfig config, SimulationTime simulationTime, uint nowTick)
        {
            ref readonly var data = ref request.Read<CombatAbilityRequest>();
            if (!data.Source.TryUnpack<ServerWT>(out var source)
                || !source.Has<CharacterNetState>()
                || !source.Has<ServerCombatAttackState>())
            {
                request.Destroy();
                return;
            }

            if (source.Has<PlayerTag>())
            {
                ref readonly var preparedBuild = ref source.Read<PreparedLoadoutSnapshot>();
                if (!LoadoutPreparationRules.IsAbilityPrepared(preparedBuild, data.AbilityId))
                {
                    request.Destroy();
                    return;
                }
            }

            if (!data.Target.TryUnpack<ServerWT>(out var target)
                || !target.Has<CharacterNetState>()
                || !target.Has<Health>())
            {
                request.Destroy();
                return;
            }

            if (target.Read<Health>().Current <= 0f)
            {
                request.Destroy();
                return;
            }

            var sourcePosition = source.Read<CharacterNetState>().Position;
            var targetPosition = target.Read<CharacterNetState>().Position;
            if ((sourcePosition - targetPosition).sqrMagnitude > GetRange(data.AbilityId, config) * GetRange(data.AbilityId, config))
            {
                request.Destroy();
                return;
            }

            ref var attackState = ref source.Mut<ServerCombatAttackState>();
            if (data.ClientCommandId != 0 && data.ClientCommandId <= attackState.LastAcceptedShotSequence)
            {
                request.Destroy();
                return;
            }

            if (nowTick < attackState.NextAttackTick)
            {
                request.Destroy();
                return;
            }

            if (data.ClientCommandId != 0)
                attackState.LastAcceptedShotSequence = data.ClientCommandId;

            attackState.NextAttackTick = simulationTime.DeadlineAfter(Mathf.Max(0f, GetCooldown(data.AbilityId, config)));
            request.Set<CombatAbilityValidatedTag>();
        }

        private static float GetRange(CombatAbilityId abilityId, CombatConfig config)
        {
            switch (abilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    return config.PoisonArrowRange;
                case CombatAbilityId.FireFlask:
                    return config.FireFlaskRange;
                default:
                    return config.Radius;
            }
        }

        private static float GetCooldown(CombatAbilityId abilityId, CombatConfig config)
        {
            switch (abilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    return config.PoisonArrowCooldown;
                case CombatAbilityId.FireFlask:
                    return config.FireFlaskCooldown;
                default:
                    return config.FireInterval;
            }
        }
    }
}
