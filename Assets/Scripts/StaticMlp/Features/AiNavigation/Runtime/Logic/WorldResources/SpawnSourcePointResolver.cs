using StaticMlp.Features.CombatDirector;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class SpawnSourcePointResolver : ISpawnSourcePointResolver
    {
        public float3 Resolve(in SpawnSource source, in CombatCellNavArea navArea, int navVersion)
        {
            var offset = source.Position - navArea.Center;
            var distance = math.length(offset);

            if (distance <= navArea.NavBuildRadius)
                return source.Position;

            var direction = math.normalize(offset);
            return navArea.Center + direction * navArea.NavBuildRadius;
        }
    }
}
