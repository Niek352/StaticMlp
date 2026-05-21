using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public struct DepositConstructionResourcesResultEvent : IEvent, IRequestResult, IEventConfig<DepositConstructionResourcesResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 32404;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Site;
        public ResourceAmount[] AcceptedResources;

        public EventTypeConfig<DepositConstructionResourcesResultEvent> Config() =>
            new(guid: new Guid("f5910f59-cf18-4370-bfad-7251af0cb3d4"));
    }
}
