using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public struct OwnerLoadoutSelection : IComponent
    {
        public LoadoutModuleId PrimaryModuleId;
    }
}
