using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationLogicFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new CombatCellNavAreaSyncSystem(), GameplaySystemOrder.Gameplay - 97);
            systems.Add(new RuntimeNavMeshRebuildQueueSystem(), GameplaySystemOrder.Gameplay - 96);
            systems.Add(new RuntimeNavMeshBuildSystem(), GameplaySystemOrder.Gameplay - 84);
        }
    }
}
