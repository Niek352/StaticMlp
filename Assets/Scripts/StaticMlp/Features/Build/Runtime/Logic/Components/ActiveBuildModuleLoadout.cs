using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Build
{
    public struct ActiveBuildModuleLoadout : IComponent
    {
        public BuildModuleId Combat0;
        public BuildModuleId Combat1;
        public BuildModuleId Combat2;
        public BuildModuleId Utility0;
        public BuildModuleId Utility1;
        public BuildModuleId BuildSignal0;
        public BuildModuleId BuildSignal1;
        public BuildModuleId BaseInfrastructure0;
        public BuildModuleId BaseInfrastructure1;
        public BuildModuleId BaseInfrastructure2;
        public BuildModuleId BaseInfrastructure3;
    }
}
