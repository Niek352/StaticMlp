using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Loadout;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionViewModel : EcsWindowViewModelBase
    {
        public ExpeditionSelectionScreenState State { get; private set; }

        public static string DescribePreparedBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Sync(in ExpeditionSelectionScreenState state)
        {
            State = state;
            NotifyChanged();
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
