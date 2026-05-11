using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionSiteQuery
    {
        public static bool TryGetConstructionSite(EntityGID gid, out SW.Entity site)
        {
            if (gid.TryUnpack<ServerWT>(out site)
                && site.Has<ConstructionSiteTag>()
                && site.Has<ConstructionSiteState>()
                && site.Has<ConstructionResources>()
                && site.Has<ConstructionTransform>())
                return true;

            site = default;
            return false;
        }

        public static bool TryGetBuildableSite(EntityGID gid, out SW.Entity site)
        {
            return TryGetConstructionSite(gid, out site)
                   && site.Has<ConstructionProgress>();
        }
    }
}
