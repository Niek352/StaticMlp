using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierNavWorkPhaseSyncSystem : ISystem
    {
        private readonly List<EntityGID> _staleStates = new();

        public void Update()
        {
            foreach (var entity in SW.Query<All<ThreatState>>().Entities())
                Sync(entity);

            foreach (var entity in SW.Query<All<FrontierNavWorkPhaseState>, None<ThreatState>>().Entities())
                _staleStates.Add(entity.GID);

            for (var i = 0; i < _staleStates.Count; i++)
            {
                if (_staleStates[i].TryUnpack<ServerWT>(out var entity) && entity.Has<FrontierNavWorkPhaseState>())
                    entity.Delete<FrontierNavWorkPhaseState>();
            }

            _staleStates.Clear();
        }

        private static void Sync(SW.Entity entity)
        {
            var next = new FrontierNavWorkPhaseState
            {
                Phase = Map(entity.Read<ThreatState>().Phase)
            };

            if (entity.Has<FrontierNavWorkPhaseState>())
            {
                ref var current = ref entity.Mut<FrontierNavWorkPhaseState>();
                current = next;
                return;
            }

            entity.Set(next);
        }

        private static FrontierNavWorkPhase Map(ThreatPhase phase)
        {
            return phase switch
            {
                ThreatPhase.Calm => FrontierNavWorkPhase.Calm,
                ThreatPhase.Rising => FrontierNavWorkPhase.BuildUp,
                ThreatPhase.RaidPending => FrontierNavWorkPhase.Peak,
                ThreatPhase.RaidActive => FrontierNavWorkPhase.Peak,
                _ => FrontierNavWorkPhase.Peak
            };
        }
    }
}
