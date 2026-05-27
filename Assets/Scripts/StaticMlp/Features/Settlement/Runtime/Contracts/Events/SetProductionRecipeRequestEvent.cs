using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public struct SetProductionRecipeRequestEvent : IEvent,
        IRequest<SetProductionRecipeResultEvent>,
        IEventConfig<SetProductionRecipeRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40183;

        public RequestId RequestId { get; set; }
        public EntityGID Building { get; set; }
        public ushort RecipeId { get; set; }

        public SetProductionRecipeRequestEvent(EntityGID building, ushort recipeId)
        {
            RequestId = default;
            Building = building;
            RecipeId = recipeId;
        }

        public EventTypeConfig<SetProductionRecipeRequestEvent> Config() =>
            new(guid: new Guid("69bdee89-872f-4804-8f31-a266a82808c0"));
    }
}
