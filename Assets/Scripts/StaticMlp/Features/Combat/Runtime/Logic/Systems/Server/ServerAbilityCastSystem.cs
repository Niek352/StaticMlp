using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerAbilityCastSystem : ISystem
    {
        private readonly List<EntityGID> _requests = new();

        public void Update()
        {
            _requests.Clear();
            foreach (var request in SW.Query<All<CombatAbilityRequest, CombatAbilityValidatedTag>>().Entities())
                _requests.Add(request.GID);

            for (var i = 0; i < _requests.Count; i++)
            {
                if (!_requests[i].TryUnpack<ServerWT>(out var request))
                    continue;

                Cast(request);
            }
        }

        private static void Cast(SW.Entity request)
        {
            ref readonly var data = ref request.Read<CombatAbilityRequest>();
            if (data.Target.Kind == CombatTargetKind.StaticPlacement)
            {
                SW.SendEvent(new CombatTargetHitEvent
                {
                    Source = data.Source,
                    Target = data.Target,
                    AbilityId = data.AbilityId,
                    ClientCommandId = data.ClientCommandId
                });
            }
            else
            {
                var hit = SW.NewEntity<Default>();
                hit.Set(new CombatHit
                {
                    AbilityId = data.AbilityId,
                    Source = data.Source,
                    Target = data.Target,
                    ClientCommandId = data.ClientCommandId,
                });
            }

            request.Destroy();
        }
    }
}
