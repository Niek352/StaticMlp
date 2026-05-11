using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Build
{
    public struct PrepareBuildCommand : IEvent
    {
        public BuildModuleId PrimaryModuleId;

        public PrepareBuildCommand(BuildModuleId primaryModuleId)
        {
            PrimaryModuleId = primaryModuleId;
        }
    }
}
