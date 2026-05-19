using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public struct CombatCell : IComponent
    {
        public int CellId;
        public float3 Center;
        public float Radius;
    }
}
