using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public interface ISpawnSourcePointResolver : IResource
    {
        float3 Resolve(in SpawnSource source, in CombatCellNavArea navArea, int navVersion);
    }
}
