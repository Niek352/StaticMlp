using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcAcquisitionRejectedEvent : IEvent, IEventConfig<NpcAcquisitionRejectedEvent>
    {
        public readonly EntityGID Target;
        public readonly NpcAcquisitionPath Path;

        public NpcAcquisitionRejectedEvent(EntityGID target, NpcAcquisitionPath path)
        {
            Target = target;
            Path = path;
        }

        public EventTypeConfig<NpcAcquisitionRejectedEvent> Config() =>
            new(guid: new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"));
    }
}
