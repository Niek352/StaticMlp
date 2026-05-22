using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct CollectExtractionOutputResultEvent : IEvent, IRequestResult,
        IEventConfig<CollectExtractionOutputResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40176;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Building { get; set; }
        public ushort ResourceId { get; set; }
        public int TransferredAmount { get; set; }

        public ResourceId Resource => new(ResourceId);

        public EventTypeConfig<CollectExtractionOutputResultEvent> Config() =>
            new(guid: new Guid("da08195c-38df-4d75-a487-9de75ffe9bb5"));
    }
}
