using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public struct SpawnRequest : IComponent
    {
        public EntityGID SourceEntity;
        public float3 SpawnPosition;
        public int CellId;
        public int Count;
        public SpawnSourceType SourceType;
        public SpawnSourceKind SourceKind;
        public EnemyRole Role;
    }
}
