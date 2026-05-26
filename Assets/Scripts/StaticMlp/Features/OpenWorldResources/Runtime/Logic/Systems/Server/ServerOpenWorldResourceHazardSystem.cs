using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Shared;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceHazardSystem : ISystem
    {
        private readonly List<EntityGID> _targets = new();
        private EventReceiver<ServerWT, OpenWorldResourceDepletionHazardEvent> _hazards;

        public void Init()
        {
            _hazards = SW.RegisterEventReceiver<OpenWorldResourceDepletionHazardEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _hazards);
        }

        public void Update()
        {
            foreach (var hazard in _hazards)
                ApplyHazard(hazard.Value);
        }

        private void ApplyHazard(OpenWorldResourceDepletionHazardEvent evt)
        {
            ref readonly var definition = ref OpenWorldResourceHazardCatalog.Get(evt.KindId);
            var radiusSq = definition.Radius * definition.Radius;

            _targets.Clear();
            foreach (var target in SW.Query<All<CharacterNetState, Health>>().Entities())
            {
                if (target.Read<Health>().Current <= 0f)
                    continue;

                var delta = target.Read<CharacterNetState>().Position - evt.Origin;
                if (delta.sqrMagnitude > radiusSq)
                    continue;

                _targets.Add(target.GID);
            }

            for (var i = 0; i < _targets.Count; i++)
                EffectCommands.CreateDamage(evt.SourcePlayer, _targets[i], definition.Damage, definition.DamageType);
        }
    }
}
