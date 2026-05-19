using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationLogicFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new CombatCellNavAreaSyncSystem(), GameplaySystemOrder.Gameplay - 97);
            systems.Add(new RuntimeNavMeshRebuildQueueSystem(), GameplaySystemOrder.Gameplay - 96);
            // RuntimeNavMeshBuildSystem is intentionally not registered until the runtime NavMesh backend exists.
            systems.Add(new SpawnSourceReachabilitySystem(), GameplaySystemOrder.Gameplay - 83);
            systems.Add(new ReachableSpawnSourceCacheSystem(), GameplaySystemOrder.Gameplay - 82);
            systems.Add(new ResolvedSpawnPointSystem(), GameplaySystemOrder.Gameplay - 81);
            systems.Add(new FarAiApproximateMoveSystem(), GameplaySystemOrder.Gameplay - 38);
            systems.Add(new LocalNavAttachStateSystem(), GameplaySystemOrder.Gameplay - 37);
            systems.Add(new FarToLocalAiNavigationHandoffSystem(), GameplaySystemOrder.Gameplay - 36);
            systems.Add(new AiNavigationDebugSnapshotSystem(), GameplaySystemOrder.Gameplay - 34);
        }
    }
}
