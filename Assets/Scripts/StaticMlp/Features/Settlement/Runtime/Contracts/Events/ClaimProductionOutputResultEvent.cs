using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public struct ClaimProductionOutputResultEvent : IEvent, IRequestResult,
        IEventConfig<ClaimProductionOutputResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40180;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Building { get; set; }
        public ushort ResourceId { get; set; }
        public int TransferredAmount { get; set; }

        public ResourceId Resource => new(ResourceId);

        public EventTypeConfig<ClaimProductionOutputResultEvent> Config() =>
            new(guid: new Guid("fc29ab6d-b684-4536-bad6-527667fe3893"));
    }
}
