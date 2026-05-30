using System;
using Aspid.MVVM;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    [ViewModel]
    public sealed partial class SettlementHudViewModel
    {
        [OneWayBind] private SettlementHudViewData _data;

        public event Action Changed;

        public SettlementHudState Settlement => Data.Settlement;
        public ExpeditionHudState Expedition => Data.Expedition;
        public LoadoutHudState Loadout => Data.Loadout;
        public ThreatHudState Threat => Data.Threat;
        public RaidHudState Raid => Data.Raid;
        public BossHudState Boss => Data.Boss;
        public ProgressionHudState Progression => Data.Progression;

        public static string DescribeBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(in SettlementHudViewData data)
        {
            Data = data;
        }

        partial void OnDataChanged(SettlementHudViewData newValue)
        {
            Changed?.Invoke();
        }

        public void OpenLoadoutPreparation()
        {
            CW.SendEvent(new SettlementHudOpenLoadoutPreparationIntent());
        }

        public void OpenExpeditionSelection()
        {
            CW.SendEvent(new SettlementHudOpenExpeditionSelectionIntent());
        }

        public void CloseHud()
        {
            CW.SendEvent(new SettlementHudCloseIntent());
        }
    }
}
