using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiBehaviorCatalog : IResource
    {
        private readonly Dictionary<ushort, AiBehaviorDefinition> _behaviorsById;

        public AiBehaviorCatalog(AiBehaviorDefinition[] behaviors)
        {
            Behaviors = behaviors ?? Array.Empty<AiBehaviorDefinition>();
            _behaviorsById = new Dictionary<ushort, AiBehaviorDefinition>(Behaviors.Length);

            for (var i = 0; i < Behaviors.Length; i++)
                _behaviorsById[Behaviors[i].BehaviorId] = Behaviors[i];
        }

        public AiBehaviorDefinition[] Behaviors { get; }

        public bool TryGetBehavior(ushort behaviorId, out AiBehaviorDefinition behavior)
        {
            return _behaviorsById.TryGetValue(behaviorId, out behavior);
        }
    }
}
