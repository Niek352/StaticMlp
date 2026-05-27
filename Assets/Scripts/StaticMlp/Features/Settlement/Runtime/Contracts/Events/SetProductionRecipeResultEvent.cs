using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public struct SetProductionRecipeResultEvent : IEvent, IRequestResult,
        IEventConfig<SetProductionRecipeResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40184;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Building { get; set; }
        public ushort ActiveRecipeId { get; set; }

        public EventTypeConfig<SetProductionRecipeResultEvent> Config() =>
            new(guid: new Guid("5840afa6-1ce5-4156-b8e3-4d734a965b69"));
    }
}
