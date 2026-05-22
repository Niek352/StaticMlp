using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public struct LoadoutHudState : IResource
    {
        public LoadoutModuleId PreparedPrimaryModuleId;
        public bool HasPreparedBuild;
    }
}
