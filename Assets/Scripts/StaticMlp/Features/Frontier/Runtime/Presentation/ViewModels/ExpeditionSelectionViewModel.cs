using System;
using Aspid.MVVM;
using StaticMlp.Features.Loadout;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    [ViewModel]
    public sealed partial class ExpeditionSelectionViewModel
    {
        [OneWayBind] private ExpeditionSelectionScreenState _state;

        public event Action Changed;

        public static string DescribePreparedBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(in ExpeditionSelectionViewData data)
        {
            State = data.State;
        }

        partial void OnStateChanged(ExpeditionSelectionScreenState newValue)
        {
            Changed?.Invoke();
        }

        public void StartExpedition()
        {
            CW.SendEvent(new ExpeditionSelectionStartIntent(
                State.AnchorId,
                State.ExpeditionId,
                State.BossId,
                State.IsBossEncounterMode));
        }

        public void Close()
        {
            CW.SendEvent(new ExpeditionSelectionCloseIntent());
        }
    }
}
