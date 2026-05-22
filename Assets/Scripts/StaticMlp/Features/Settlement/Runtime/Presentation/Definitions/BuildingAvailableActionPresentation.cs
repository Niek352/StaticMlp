using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingAvailableActionPresentation
    {
        public BuildingInteractionKind Kind;
        public string Label;
        public string InputHint;
        public string EffectDescription;
        public bool Enabled;
        public string DisabledReason;
        public EntityGID Target;

        public BuildingAvailableActionPresentation(
            BuildingInteractionKind kind,
            string label,
            string inputHint,
            string effectDescription,
            bool enabled,
            string disabledReason,
            EntityGID target)
        {
            Kind = kind;
            Label = label;
            InputHint = inputHint;
            EffectDescription = effectDescription;
            Enabled = enabled;
            DisabledReason = disabledReason;
            Target = target;
        }

        public bool IsDefined => Kind != BuildingInteractionKind.None;
    }
}
