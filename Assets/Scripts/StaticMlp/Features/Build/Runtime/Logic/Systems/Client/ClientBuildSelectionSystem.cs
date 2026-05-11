using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Build
{
    public sealed class ClientBuildSelectionSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag>>().Entities())
            {
                if (!player.Has<OwnerBuildSelection>())
                    player.Set(Stage1BuildRules.DefaultSelection());

                if (!player.Has<ClientBuildSelectionSyncState>())
                    player.Set(new ClientBuildSelectionSyncState());

                ref var selection = ref player.Mut<OwnerBuildSelection>();
                if (inputState.WasPressed(CoreInputActions.Next))
                    selection.PrimaryModuleId = Stage1BuildRules.NextPrimaryModule(selection.PrimaryModuleId);
                else if (inputState.WasPressed(CoreInputActions.Previous))
                    selection.PrimaryModuleId = Stage1BuildRules.PreviousPrimaryModule(selection.PrimaryModuleId);

                player.Set(Stage1BuildRules.CreatePreparedSnapshot(selection));

                ref var syncState = ref player.Mut<ClientBuildSelectionSyncState>();
                if (syncState.LastSentPrimaryModuleId == selection.PrimaryModuleId)
                    continue;

                var command = new PrepareBuildCommand(selection.PrimaryModuleId);
                if (!CW.SendToServerEvent(in command))
                    continue;

                syncState.LastSentPrimaryModuleId = selection.PrimaryModuleId;
            }
        }
    }
}
