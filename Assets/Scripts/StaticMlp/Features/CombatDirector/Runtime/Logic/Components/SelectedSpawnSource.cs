using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public struct SelectedSpawnSource : IComponent
    {
        public int CellId;
        public EntityGID SourceEntity;
        public SpawnSourceType SourceType;
        public SpawnSourceKind SourceKind;
        public float3 Position;
    }
}
