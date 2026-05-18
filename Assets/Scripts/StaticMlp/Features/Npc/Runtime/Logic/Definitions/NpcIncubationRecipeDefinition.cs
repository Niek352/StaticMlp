using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcIncubationRecipeDefinition
    {
        public readonly NpcIncubationRecipeId Id;
        public readonly NpcDefinitionId ResultNpc;
        public readonly ResourceAmount[] Costs;
        public readonly byte RequiredStationTier;
        public readonly float DurationSeconds;

        public NpcIncubationRecipeDefinition(
            NpcIncubationRecipeId id,
            NpcDefinitionId resultNpc,
            ResourceAmount[] costs,
            byte requiredStationTier,
            float durationSeconds)
        {
            Id = id;
            ResultNpc = resultNpc;
            Costs = costs;
            RequiredStationTier = requiredStationTier;
            DurationSeconds = durationSeconds;
        }
    }
}
