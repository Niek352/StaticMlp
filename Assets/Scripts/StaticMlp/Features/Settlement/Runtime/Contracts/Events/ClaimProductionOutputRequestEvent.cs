using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public struct ClaimProductionOutputRequestEvent : IEvent,
        IRequest<ClaimProductionOutputResultEvent>,
        IEventConfig<ClaimProductionOutputRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40179;

        public RequestId RequestId { get; set; }
        public EntityGID Building { get; set; }
        public ushort ResourceId { get; set; }
        public int RequestedAmount { get; set; }

        public ResourceId Resource => new(ResourceId);

        public ClaimProductionOutputRequestEvent(EntityGID building, ResourceId resource, int requestedAmount)
        {
            RequestId = default;
            Building = building;
            ResourceId = resource.Value;
            RequestedAmount = requestedAmount;
        }

        public EventTypeConfig<ClaimProductionOutputRequestEvent> Config() =>
            new(guid: new Guid("ccc0e591-1c90-44ce-ac59-047f5d0ae639"));
    }
}
