using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatPresentationFeature : GameplayFeature
    {
        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientCombatPresentationConfigInitSystem(), GameplaySystemOrder.Gameplay - 9);
            systems.Add(new ClientPassiveAutoAttackPresentationSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientCombatHealthViewStateSystem(), (short)(ViewSystemOrder.BuildPresentationState + 5));
            systems.Add(new ClientDamageFeedbackPresentationSystem(), (short)(ViewSystemOrder.BuildPresentationState + 10));
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<CombatHealthViewState>();
            views.Register<PassiveAutoAttackViewState>();
            views.Register<PassiveAutoAttackTargetViewState>();
            views.Register<DamageFeedbackViewState>();
        }
    }
}
