using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class ServerAreaEffectTickSystem : ISystem
    {
        private readonly List<EntityGID> _areas = new();

        public void Update()
        {
            _areas.Clear();
            foreach (var area in SW.Query<All<AreaEffectTag, AreaEffectState, LifeTime>, None<IsDestroyed>>().Entities())
                _areas.Add(area.GID);

            var currentTick = SW.GetResource<SimulationTime>().ServerTick;
            for (var i = 0; i < _areas.Count; i++)
            {
                if (!_areas[i].TryUnpack<ServerWT>(out var area))
                    continue;

                TickArea(area, currentTick);
            }
        }

        private static void TickArea(SW.Entity area, uint currentTick)
        {
            ref var state = ref area.Mut<AreaEffectState>();
            ref readonly var lifeTime = ref area.Read<LifeTime>();
            if (currentTick >= lifeTime.EndTick)
                return;

            if (state.TickIntervalTicks == 0)
                return;

            var radiusSq = state.Radius * state.Radius;
            while (currentTick >= state.NextDamageTick && currentTick < lifeTime.EndTick)
            {
                state.NextDamageTick += state.TickIntervalTicks;
                foreach (var target in SW.Query<All<CharacterNetState, Health>>().Entities())
                {
                    if (target.GID == state.Source || target.Read<Health>().Current <= 0f)
                        continue;

                    var delta = target.Read<CharacterNetState>().Position - state.Position;
                    if (delta.sqrMagnitude > radiusSq)
                        continue;

                    EffectCommands.CreateDamage(
                        state.Source,
                        target.GID,
                        state.DamagePerTick,
                        state.DamageType,
                        state.RequestId,
                        state.RootEffectId,
                        (byte)(state.ChainDepth + 1),
                        state.MaxDepth);
                }
            }
        }
    }
}
