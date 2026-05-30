using System;
using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CampFlow;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutPreparationIntentSystem : ISystem
    {
        private WindowsController<ClientCoreWT> _windows;
        private EventReceiver<ClientCoreWT, LoadoutPreparationSelectModuleIntent> _selectModuleIntents;
        private EventReceiver<ClientCoreWT, LoadoutPreparationConfirmIntent> _confirmIntents;
        private EventReceiver<ClientCoreWT, LoadoutPreparationCloseIntent> _closeIntents;

        public void Init()
        {
            _windows = CW.GetResource<WindowsController<ClientCoreWT>>();
            _selectModuleIntents = CW.RegisterEventReceiver<LoadoutPreparationSelectModuleIntent>();
            _confirmIntents = CW.RegisterEventReceiver<LoadoutPreparationConfirmIntent>();
            _closeIntents = CW.RegisterEventReceiver<LoadoutPreparationCloseIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _selectModuleIntents);
            CW.DeleteEventReceiver(ref _confirmIntents);
            CW.DeleteEventReceiver(ref _closeIntents);
        }

        public void Update()
        {
            foreach (var evt in _selectModuleIntents)
                SelectModule(evt.Value.ModuleId);

            foreach (var _ in _confirmIntents)
                ConfirmBuild();

            foreach (var _ in _closeIntents)
                Close();
        }

        private static void SelectModule(LoadoutModuleId moduleId)
        {
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, OwnerLoadoutSelection>>().Entities())
            {
                ref var selection = ref player.Mut<OwnerLoadoutSelection>();
                selection.PrimaryModuleId = moduleId;
                player.Set(LoadoutPreparationRules.CreatePreparedSnapshot(selection));
                return;
            }

            throw new InvalidOperationException("Build preparation requires a local player with owner build selection.");
        }

        private void ConfirmBuild()
        {
            if (CanPrepareBoss())
            {
                var request = new PrepareBossRequestEvent(SettlementAnchorCatalog.HomeCampId);
                CW.SendToServer(in request);
                Close();
                return;
            }

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, ClientLoadoutSelectionSyncState>>().Entities())
            {
                ref var syncState = ref player.Mut<ClientLoadoutSelectionSyncState>();
                syncState.ShouldCommitSelection = true;
                Close();
                return;
            }

            throw new InvalidOperationException("Build preparation confirmation requires a local player sync state.");
        }

        private static bool CanPrepareBoss()
        {
            if (!CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return false;

            if (!anchor.Has<Projected<ProgressionState>>())
                return false;

            ref readonly var progression = ref ClientProjection.Read<ProgressionState>(anchor);
            return progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                   && !progression.HasFlag(ProgressFlagCatalog.BossUnlockedId)
                   && progression.HasBossPreparationToken();
        }

        private void Close()
        {
            _windows.TryClose<LoadoutPreparationWindow>();
        }
    }
}
