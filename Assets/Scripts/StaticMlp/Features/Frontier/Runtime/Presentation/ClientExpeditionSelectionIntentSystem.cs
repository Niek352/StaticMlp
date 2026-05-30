using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientExpeditionSelectionIntentSystem : ISystem
    {
        private WindowsController<ClientCoreWT> _windows;
        private EventReceiver<ClientCoreWT, ExpeditionSelectionStartIntent> _startIntents;
        private EventReceiver<ClientCoreWT, ExpeditionSelectionCloseIntent> _closeIntents;

        public void Init()
        {
            _windows = CW.GetResource<WindowsController<ClientCoreWT>>();
            _startIntents = CW.RegisterEventReceiver<ExpeditionSelectionStartIntent>();
            _closeIntents = CW.RegisterEventReceiver<ExpeditionSelectionCloseIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _startIntents);
            CW.DeleteEventReceiver(ref _closeIntents);
        }

        public void Update()
        {
            foreach (var evt in _startIntents)
                StartExpedition(evt.Value);

            foreach (var _ in _closeIntents)
                Close();
        }

        private void StartExpedition(in ExpeditionSelectionStartIntent intent)
        {
            if (intent.IsBossEncounterMode)
            {
                var bossRequest = new StartBossEncounterRequestEvent(intent.AnchorId, intent.BossId);
                CW.SendToServer(in bossRequest);
                Close();
                return;
            }

            var request = new StartExpeditionRequestEvent(intent.AnchorId, intent.ExpeditionId);
            CW.SendToServer(in request);
            Close();
        }

        private void Close()
        {
            _windows.TryClose<ExpeditionSelectionWindow>();
        }
    }
}
