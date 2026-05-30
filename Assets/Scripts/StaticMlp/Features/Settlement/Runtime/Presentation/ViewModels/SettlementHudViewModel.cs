using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementHudViewModel : EcsWindowViewModelBase
    {
        public SettlementHudState Settlement { get; private set; }
        public ExpeditionHudState Expedition { get; private set; }
        public LoadoutHudState Loadout { get; private set; }
        public ThreatHudState Threat { get; private set; }
        public RaidHudState Raid { get; private set; }
        public BossHudState Boss { get; private set; }
        public ProgressionHudState Progression { get; private set; }

        public static string DescribeBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Sync(
            in SettlementHudState settlement,
            in ExpeditionHudState expedition,
            in LoadoutHudState loadout,
            in ThreatHudState threat,
            in RaidHudState raid,
            in BossHudState boss,
            in ProgressionHudState progression)
        {
            Settlement = settlement;
            Expedition = expedition;
            Loadout = loadout;
            Threat = threat;
            Raid = raid;
            Boss = boss;
            Progression = progression;
            NotifyChanged();
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
