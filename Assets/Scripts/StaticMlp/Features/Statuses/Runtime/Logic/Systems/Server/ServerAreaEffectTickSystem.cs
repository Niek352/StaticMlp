using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Statuses
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
            foreach (var area in SW.Query<All<AreaEffectTag, AreaEffectState, LifeTime>, None<IsDestroyed>>().Entities())
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
            ref var lifeTime = ref area.Mut<LifeTime>();
            lifeTime.RemainingTime -= deltaTime;
            state.TickTimer += deltaTime;
            if (lifeTime.RemainingTime <= 0f)
                return;

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
