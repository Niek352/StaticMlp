using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct EnemySpawnSource : IComponent
    {
        public EntityGID SourceEntity;
        public SpawnSourceType SourceType;
    }
}
