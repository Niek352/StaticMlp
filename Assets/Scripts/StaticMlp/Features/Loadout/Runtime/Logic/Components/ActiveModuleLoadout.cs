using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public struct ActiveModuleLoadout : IComponent
    {
        public LoadoutModuleId Combat0;
        public LoadoutModuleId Combat1;
        public LoadoutModuleId Combat2;
        public LoadoutModuleId Utility0;
        public LoadoutModuleId Utility1;
        public LoadoutModuleId BuildSignal0;
        public LoadoutModuleId BuildSignal1;
        public LoadoutModuleId BaseInfrastructure0;
        public LoadoutModuleId BaseInfrastructure1;
        public LoadoutModuleId BaseInfrastructure2;
        public LoadoutModuleId BaseInfrastructure3;
    }
}
