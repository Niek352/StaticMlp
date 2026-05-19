using FFS.Libraries.StaticEcs;
using Unity.Mathematics;

namespace StaticMlp.Features.Frontier
{
    public struct CombatCell : IComponent
    {
        public int CellId;
        public float3 Center;
        public float Radius;
    }
}
