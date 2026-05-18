using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcAcquisitionAcceptedEvent : IEvent, IEventConfig<NpcAcquisitionAcceptedEvent>
    {
        public readonly EntityGID RosterRecord;
        public readonly NpcAcquisitionPath Path;

        public NpcAcquisitionAcceptedEvent(EntityGID rosterRecord, NpcAcquisitionPath path)
        {
            RosterRecord = rosterRecord;
            Path = path;
        }

        public EventTypeConfig<NpcAcquisitionAcceptedEvent> Config() =>
            new(guid: new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"));
    }
}
