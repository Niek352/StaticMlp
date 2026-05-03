using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public static class BuildingMenuStateUtility
    {
        public static CW.Entity GetOrCreate()
        {
            foreach (var e in CW.Query<All<BuildingMenuState>>().Entities())
                return e;

            EnsureClientOnlyStorage();
            var created = CW.NewEntityInChunk<Default>(ClientBuildingClusters.ClientOnlyChunk);
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

        private static void EnsureClientOnlyStorage()
        {
            if (!CW.ClusterIsRegistered(ClientBuildingClusters.ClientOnly))
                CW.RegisterCluster(ClientBuildingClusters.ClientOnly);

            if (!CW.ChunkIsRegistered(ClientBuildingClusters.ClientOnlyChunk))
                CW.RegisterChunk(ClientBuildingClusters.ClientOnlyChunk, ChunkOwnerType.Self, ClientBuildingClusters.ClientOnly);
        }
    }
}
