using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct CollectExtractionOutputRequestEvent : IEvent,
        IRequest<CollectExtractionOutputResultEvent>, IEventConfig<CollectExtractionOutputRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40175;

        public RequestId RequestId { get; set; }
        public EntityGID Building { get; set; }
        public ushort ResourceId { get; set; }
        public int RequestedAmount { get; set; }

        public ResourceId Resource => new(ResourceId);

        public CollectExtractionOutputRequestEvent(EntityGID building, ResourceId resource, int requestedAmount)
        {
            RequestId = default;
            Building = building;
            ResourceId = resource.Value;
            RequestedAmount = requestedAmount;
        }

        public EventTypeConfig<CollectExtractionOutputRequestEvent> Config() =>
            new(guid: new Guid("5f01bc7a-7dbd-4b65-8adc-b120c440e628"));
    }
}
