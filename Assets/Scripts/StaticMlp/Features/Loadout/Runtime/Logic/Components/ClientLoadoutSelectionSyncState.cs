using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public struct ClientLoadoutSelectionSyncState : IComponent
    {
        public LoadoutModuleId LastSentPrimaryModuleId;
        public bool ShouldCommitSelection;
    }
}
