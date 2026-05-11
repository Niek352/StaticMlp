using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Build
{
    public struct OwnerBuildSelection : IComponent
    {
        public BuildModuleId PrimaryModuleId;
    }
}
