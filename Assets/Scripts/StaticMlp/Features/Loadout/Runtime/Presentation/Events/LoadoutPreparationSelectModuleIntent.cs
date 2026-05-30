using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public readonly struct LoadoutPreparationSelectModuleIntent : IEvent
    {
        public readonly LoadoutModuleId ModuleId;

        public LoadoutPreparationSelectModuleIntent(LoadoutModuleId moduleId)
        {
            ModuleId = moduleId;
        }
    }
}
