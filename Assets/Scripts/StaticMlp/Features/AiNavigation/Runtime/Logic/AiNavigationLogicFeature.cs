using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationLogicFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new CombatCellNavAreaSyncSystem(), GameplaySystemOrder.Gameplay - 97);
        }
    }
}
