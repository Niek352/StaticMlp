using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public static class BuildingMenuStateUtility
    {
        public static CW.Entity GetOrCreate()
        {
            foreach (var e in CW.Query<All<BuildingMenuState>>().Entities())
                return e;

            var created = ClientOnlyEntities.New(ClientBuildingClusters.ClientOnly, ClientBuildingClusters.ClientOnlyChunk);
            created.Set(new BuildingMenuState());
            return created;
        }

        public static bool TryGet(out CW.Entity entity, out BuildingMenuState state)
        {
            foreach (var e in CW.Query<All<BuildingMenuState>>().Entities())
            {
                entity = e;
                state = e.Read<BuildingMenuState>();
                return true;
            }

            entity = default;
            state = default;
            return false;
        }
    }
}
