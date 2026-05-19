using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public struct SpawnRequest : IComponent
    {
        public int CellId;
        public EnemyRole Role;
        public int Count;
        public float3 SpawnPosition;
    }
}
