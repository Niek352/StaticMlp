using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public struct DepositConstructionResourcesRequestEvent : IEvent, IRequest<DepositConstructionResourcesResultEvent>, IEventConfig<DepositConstructionResourcesRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 32403;

        public RequestId RequestId { get; set; }
        public EntityGID Site { get; set; }
        public ResourceAmount[] Resources;

        public DepositConstructionResourcesRequestEvent(EntityGID site, ResourceAmount[] resources)
        {
            RequestId = default;
            Site = site;
            Resources = resources ?? Array.Empty<ResourceAmount>();
        }

        public EventTypeConfig<DepositConstructionResourcesRequestEvent> Config() =>
            new(guid: new Guid("5d30f087-d3b5-411c-b3be-92faf9f9551f"));
    }
}
