using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Effects
{
    public sealed class EffectsPresentationFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientCombatVisualSpawnSystem(), GameplaySystemOrder.Gameplay + 36);
            systems.Add(new ClientCombatVisualLifetimeSystem(), GameplaySystemOrder.Gameplay + 37);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<CombatProjectileVisualState>();
            views.Register<CombatEffectVisualState>();
        }
    }
}
