using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Interaction
{
    public sealed class InteractionGameplayFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientInteractionBootstrapSystem(), (short)(GameplaySystemOrder.ClientInput + 1));
            systems.Add(new ClientInteractionFocusSystem(), (short)(GameplaySystemOrder.ClientInput + 15));
            systems.Add(new ClientInteractPressSystem(), (short)(GameplaySystemOrder.ClientInput + 16));
        }
    }
}
