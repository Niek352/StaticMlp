using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public interface IBuildingInteractionHandler
    {
        bool CanHandle(BuildingInteractionKind kind);
        string ResolveInputHint(BuildingInteractionKind kind, bool isPrimaryAction);
        string ResolveEffectDescription(BuildingInteractionKind kind);
        string ResolveSummary(BuildingInteractionKind kind, in BuildingDefinition definition);
        void Execute(EntityGID target, BuildingInteractionKind kind);
    }
}
