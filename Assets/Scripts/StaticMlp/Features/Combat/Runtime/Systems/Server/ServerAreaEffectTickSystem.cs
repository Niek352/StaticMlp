using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerAreaEffectTickSystem : ISystem
    {
        private readonly Func<float> _deltaTimeProvider;
        private readonly List<EntityGID> _areas = new();

        public ServerAreaEffectTickSystem(Func<float> deltaTimeProvider = null)
        {
            _deltaTimeProvider = deltaTimeProvider ?? (() => Time.deltaTime);
        }

        public void Update()
        {
            _areas.Clear();
            foreach (var area in SW.Query<All<AreaEffectTag, AreaEffectState>>().Entities())
                _areas.Add(area.GID);

            var deltaTime = Mathf.Max(0f, _deltaTimeProvider());
            for (var i = 0; i < _areas.Count; i++)
            {
                if (!_areas[i].TryUnpack<ServerWT>(out var area))
                    continue;

                TickArea(area, deltaTime);
            }
        }

        private static void TickArea(SW.Entity area, float deltaTime)
        {
            ref var state = ref area.Mut<AreaEffectState>();
            state.RemainingTime -= deltaTime;
            state.TickTimer += deltaTime;
            if (state.RemainingTime <= 0f)
            {
                area.Destroy();
                return;
            }

            if (state.TickInterval <= 0f || state.TickTimer < state.TickInterval)
                return;

            state.TickTimer -= state.TickInterval;
            var radiusSq = state.Radius * state.Radius;
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
