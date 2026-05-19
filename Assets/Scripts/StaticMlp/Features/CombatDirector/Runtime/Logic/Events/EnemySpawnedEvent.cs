using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public readonly struct EnemySpawnedEvent : IEvent
    {
        public readonly EntityGID SpawnedEntity;
        public readonly EnemyRole Role;
        public readonly SpawnSourceType SourceType;

        public EnemySpawnedEvent(EntityGID spawnedEntity, EnemyRole role, SpawnSourceType sourceType)
        {
            SpawnedEntity = spawnedEntity;
            Role = role;
            SourceType = sourceType;
        }
    }
}
