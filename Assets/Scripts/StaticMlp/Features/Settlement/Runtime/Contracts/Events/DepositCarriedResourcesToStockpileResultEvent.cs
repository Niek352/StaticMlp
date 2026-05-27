using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public struct DepositCarriedResourcesToStockpileResultEvent : IEvent, IRequestResult,
        IEventConfig<DepositCarriedResourcesToStockpileResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40178;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Building { get; set; }
        public int TransferredAmount { get; set; }

        public EventTypeConfig<DepositCarriedResourcesToStockpileResultEvent> Config() =>
            new(guid: new Guid("9d390ef1-31c4-411f-a5f6-c5725f647a33"));
    }
}
