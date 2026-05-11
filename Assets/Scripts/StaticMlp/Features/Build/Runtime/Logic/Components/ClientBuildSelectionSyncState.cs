using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Build
{
    public struct ClientBuildSelectionSyncState : IComponent
    {
        public BuildModuleId LastSentPrimaryModuleId;
    }
}
