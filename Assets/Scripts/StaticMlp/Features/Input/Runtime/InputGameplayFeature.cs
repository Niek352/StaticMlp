using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Input
{
    public sealed class InputGameplayFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientInputCaptureSystem(), GameplaySystemOrder.ClientInput);
            systems.Add(new ClientInputEventPublishSystem(), (short)(GameplaySystemOrder.ClientInput + 10));
        }
    }
}
