using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionSiteQuery
    {
        public static bool TryGetServerConstructionSite(EntityGID gid, out SW.Entity site)
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

        public static bool TryGetServerBuildableSite(EntityGID gid, out SW.Entity site)
        {
            return TryGetServerConstructionSite(gid, out site)
                   && site.Has<ConstructionProgress>();
        }
    }
}
