using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutSelectionSystem : ISystem
    {
        public void Update()
        {
            var isBossBuildCommitted = IsBossBuildCommitted();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag>>().Entities())
            {
                if (!player.Has<OwnerLoadoutSelection>())
                    player.Set(LoadoutPreparationRules.DefaultSelection());

                if (!player.Has<ClientLoadoutSelectionSyncState>())
                    player.Set(new ClientLoadoutSelectionSyncState());

                ref var selection = ref player.Mut<OwnerLoadoutSelection>();
                player.Set(LoadoutPreparationRules.CreatePreparedSnapshot(selection));

                ref var syncState = ref player.Mut<ClientLoadoutSelectionSyncState>();
                if (!syncState.ShouldCommitSelection)
                    continue;

                syncState.ShouldCommitSelection = false;

                if (isBossBuildCommitted || syncState.LastSentPrimaryModuleId == selection.PrimaryModuleId)
                    continue;

                var command = new PrepareLoadoutCommand(SettlementAnchorCatalog.HomeCampId, selection.PrimaryModuleId);
                CW.SendToServer(in command);
                syncState.LastSentPrimaryModuleId = selection.PrimaryModuleId;
            }
        }

        private static bool IsBossBuildCommitted()
        {
            foreach (var anchor in CW.Query<All<BossLoadoutPreparationState>>().Entities())
            {
                if (anchor.Read<BossLoadoutPreparationState>().Status == BossLoadoutPreparationStatus.Committed)
                    return true;
            }

            return false;
        }
    }
}
