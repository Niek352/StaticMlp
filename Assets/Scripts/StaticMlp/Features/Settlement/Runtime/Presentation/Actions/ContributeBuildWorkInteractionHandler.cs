using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    internal sealed class ContributeBuildWorkInteractionHandler : IBuildingInteractionHandler
    {
        public bool CanHandle(BuildingInteractionKind kind)
        {
            return kind == BuildingInteractionKind.ContributeBuildWork;
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
            var request = new BuildConstructionRequestEvent(
                target,
                ConstructionActionProfiles.PlayerBuildClickWork);
            RequestApi.Send<BuildConstructionRequestEvent, BuildConstructionResultEvent>(request);
        }
    }
}
