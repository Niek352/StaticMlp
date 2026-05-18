using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcIncubationJobCompletedEvent : IEvent, IEventConfig<NpcIncubationJobCompletedEvent>
    {
        public readonly EntityGID JobEntity;
        public readonly EntityGID RosterRecord;

        public NpcIncubationJobCompletedEvent(EntityGID jobEntity, EntityGID rosterRecord)
        {
            JobEntity = jobEntity;
            RosterRecord = rosterRecord;
        }

        public EventTypeConfig<NpcIncubationJobCompletedEvent> Config() =>
            new(guid: new Guid("d6e7f8a9-b0c1-4d2e-3f4a-5b6c7d8e9f0a"));
    }
}
