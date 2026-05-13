using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Build
{
    public sealed class ClientBuildSelectionSystem : ISystem
    {
        public void Update()
        {
            var isBossBuildCommitted = IsBossBuildCommitted();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag>>().Entities())
            {
                if (!player.Has<OwnerBuildSelection>())
                    player.Set(Stage1BuildRules.DefaultSelection());

                if (!player.Has<ClientBuildSelectionSyncState>())
                    player.Set(new ClientBuildSelectionSyncState());

                ref var selection = ref player.Mut<OwnerBuildSelection>();
                player.Set(Stage1BuildRules.CreatePreparedSnapshot(selection));

                ref var syncState = ref player.Mut<ClientBuildSelectionSyncState>();
                if (!syncState.ShouldCommitSelection)
                    continue;

                syncState.ShouldCommitSelection = false;

                if (isBossBuildCommitted || syncState.LastSentPrimaryModuleId == selection.PrimaryModuleId)
                    continue;

                var command = new PrepareBuildCommand(SettlementAnchorCatalog.HomeCampId, selection.PrimaryModuleId);
                CW.SendToServerEvent(in command);
                syncState.LastSentPrimaryModuleId = selection.PrimaryModuleId;
            }
        }

        private static bool IsBossBuildCommitted()
        {
            foreach (var anchor in CW.Query<All<BossBuildPreparationState>>().Entities())
            {
                if (anchor.Read<BossBuildPreparationState>().Status == BossBuildPreparationStatus.Committed)
                    return true;
            }

            return false;
        }
    }
}
