using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Effects
{
    public sealed class EffectsLogicFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerEffectsInitSystem(), GameplaySystemOrder.Gameplay - 131);
            systems.Add(new ServerEffectPreprocessSystem(), GameplaySystemOrder.Gameplay - 33);
            systems.Add(new ServerEffectCleanupSystem(), GameplaySystemOrder.Gameplay - 24);
        }
    }
}
