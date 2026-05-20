using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings {
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct DepositConstructionResourcesResultEvent : IEvent, IRequestResult, IEventConfig<DepositConstructionResourcesResultEvent> {
        public const ushort NETWORK_EVENT_ID = 32404;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Site;
        public int AcceptedWood;
        public int AcceptedStone;
        public int AcceptedPlanks;
        public int AcceptedSimpleParts;

        public EventTypeConfig<DepositConstructionResourcesResultEvent> Config() =>
            new(guid: new Guid("f5910f59-cf18-4370-bfad-7251af0cb3d4"));
    }
}
