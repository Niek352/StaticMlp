using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CombatDirector;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public interface ISpawnSourcePointResolver : IResource
    {
        float3 Resolve(in SpawnSource source, in NavInterestArea navArea, int navVersion);
    }
}
