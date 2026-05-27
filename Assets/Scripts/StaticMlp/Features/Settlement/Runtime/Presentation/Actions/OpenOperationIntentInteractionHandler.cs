using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    internal sealed class OpenOperationIntentInteractionHandler : IBuildingInteractionHandler
    {
        public bool CanHandle(BuildingInteractionKind kind)
        {
            return kind == BuildingInteractionKind.OpenDetails
                   || kind == BuildingInteractionKind.AssignWorker
                   || kind == BuildingInteractionKind.OpenProductionQueue
                   || kind == BuildingInteractionKind.SetRecipe
                   || kind == BuildingInteractionKind.ClaimOutput
                   || kind == BuildingInteractionKind.AssignBed
                   || kind == BuildingInteractionKind.ToggleEnabled
                   || kind == BuildingInteractionKind.TriggerRepair
                   || kind == BuildingInteractionKind.Extract
                   || kind == BuildingInteractionKind.Rest
                   || kind == BuildingInteractionKind.WithdrawItems;
        }

        public string ResolveInputHint(BuildingInteractionKind kind, bool isPrimaryAction)
        {
            return BuildingActionPresentationCatalog.ResolveInputHint(kind, isPrimaryAction);
        }

        public string ResolveEffectDescription(BuildingInteractionKind kind)
        {
            return BuildingActionPresentationCatalog.ResolveEffectDescription(kind);
        }

        public string ResolveSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return BuildingActionPresentationCatalog.ResolveSummary(kind, in definition);
        }

        public void Execute(EntityGID target, BuildingInteractionKind kind)
        {
            ref var intent = ref CW.GetResource<SettlementBuildingOperationOpenIntent>();
            intent.Set(target, kind);
        }
    }
}
