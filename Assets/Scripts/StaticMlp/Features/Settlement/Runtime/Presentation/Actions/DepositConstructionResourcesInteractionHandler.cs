using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    internal sealed class DepositConstructionResourcesInteractionHandler : IBuildingInteractionHandler
    {
        public bool CanHandle(BuildingInteractionKind kind)
        {
            return kind == BuildingInteractionKind.DepositConstructionResources;
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
            if (!target.TryUnpack<ClientCoreWT>(out var site))
                throw new System.InvalidOperationException($"Deposit construction target {target} is not a client entity.");

            var request = new DepositConstructionResourcesRequestEvent(
                target,
                ConstructionResourcesAccess.GetProjectedRemainingResources(site));
            RequestApi.Send<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>(request);
        }
    }
}
