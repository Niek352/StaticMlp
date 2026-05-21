using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingAvailableActionPresentation
    {
        public BuildingInteractionKind Kind;
        public string Label;
        public bool Enabled;
        public string DisabledReason;
        public EntityGID Target;

        public BuildingAvailableActionPresentation(
            BuildingInteractionKind kind,
            string label,
            bool enabled,
            string disabledReason,
            EntityGID target)
        {
            Kind = kind;
            Label = label;
            Enabled = enabled;
            DisabledReason = disabledReason;
            Target = target;
        }

        public bool IsDefined => Kind != BuildingInteractionKind.None;
    }
}
