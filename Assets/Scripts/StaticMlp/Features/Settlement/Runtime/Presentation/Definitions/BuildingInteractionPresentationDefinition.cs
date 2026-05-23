using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public readonly struct BuildingInteractionPresentationDefinition
    {
        public readonly BuildingInteractionKind Kind;
        public readonly string PrimaryInputHint;
        public readonly string EffectDescription;

        public BuildingInteractionPresentationDefinition(
            BuildingInteractionKind kind,
            string primaryInputHint,
            string effectDescription)
        {
            Kind = kind;
            PrimaryInputHint = primaryInputHint;
            EffectDescription = effectDescription;
        }
    }
}
