using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public struct DepositCarriedResourcesToStockpileRequestEvent : IEvent,
        IRequest<DepositCarriedResourcesToStockpileResultEvent>,
        IEventConfig<DepositCarriedResourcesToStockpileRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40177;

        public RequestId RequestId { get; set; }
        public EntityGID Building { get; set; }

        public DepositCarriedResourcesToStockpileRequestEvent(EntityGID building)
        {
            RequestId = default;
            Building = building;
        }

        public EventTypeConfig<DepositCarriedResourcesToStockpileRequestEvent> Config() =>
            new(guid: new Guid("f3375d59-1f44-427b-bd38-50cc4378a875"));
    }
}
