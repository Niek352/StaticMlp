using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementHudIntentSystem : ISystem
    {
        private WindowsController<ClientCoreWT> _windows;
        private EventReceiver<ClientCoreWT, SettlementHudOpenLoadoutPreparationIntent> _loadoutIntents;
        private EventReceiver<ClientCoreWT, SettlementHudOpenExpeditionSelectionIntent> _expeditionIntents;
        private EventReceiver<ClientCoreWT, SettlementHudCloseIntent> _closeIntents;

        public void Init()
        {
            _windows = CW.GetResource<WindowsController<ClientCoreWT>>();
            _loadoutIntents = CW.RegisterEventReceiver<SettlementHudOpenLoadoutPreparationIntent>();
            _expeditionIntents = CW.RegisterEventReceiver<SettlementHudOpenExpeditionSelectionIntent>();
            _closeIntents = CW.RegisterEventReceiver<SettlementHudCloseIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _loadoutIntents);
            CW.DeleteEventReceiver(ref _expeditionIntents);
            CW.DeleteEventReceiver(ref _closeIntents);
        }

        public void Update()
        {
            foreach (var _ in _loadoutIntents)
                _windows.Open<LoadoutPreparationWindow, EcsWindowNoData>(default);

            foreach (var _ in _expeditionIntents)
                _windows.Open<ExpeditionSelectionWindow, EcsWindowNoData>(default);

            foreach (var _ in _closeIntents)
                CloseHud();
        }

        private static void CloseHud()
        {
            ref var session = ref CW.GetResource<SettlementHudSession>();
            session.IsVisible = false;
        }
    }
}
