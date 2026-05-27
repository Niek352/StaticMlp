using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct BuildingPanelAction
    {
        public readonly BuildingPanelActionKind Kind;
        public readonly string Label;
        public readonly bool Enabled;
        public readonly string DisabledReason;
        public readonly EntityGID Target;
        public readonly EntityGID Worker;
        public readonly byte SlotIndex;
        public readonly ResourceId Resource;
        public readonly int Amount;
        public readonly ushort RecipeId;

        public BuildingPanelAction(
            BuildingPanelActionKind kind,
            string label,
            bool enabled,
            string disabledReason,
            EntityGID target,
            EntityGID worker = default,
            byte slotIndex = 0,
            ResourceId resource = default,
            int amount = 0,
            ushort recipeId = 0)
        {
            Kind = kind;
            Label = label;
            Enabled = enabled;
            DisabledReason = disabledReason;
            Target = target;
            Worker = worker;
            SlotIndex = slotIndex;
            Resource = resource;
            Amount = amount;
            RecipeId = recipeId;
        }

        public bool IsDefined => Kind != BuildingPanelActionKind.None;
    }
}
