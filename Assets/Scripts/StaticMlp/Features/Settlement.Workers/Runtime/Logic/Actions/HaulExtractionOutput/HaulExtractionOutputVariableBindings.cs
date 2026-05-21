using StaticMlp.Features.AiBots;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class HaulExtractionOutputVariableBindings
    {
        public const ushort HAS_EXTRACTION_OUTPUT_TARGET = 1503;

        public static readonly AiBlackboardFloatBinding[] Bindings =
        {
            new(HAS_EXTRACTION_OUTPUT_TARGET, ReadHasExtractionOutputTarget)
        };

        public static readonly UtilityConsideration[] Considerations =
        {
            new()
            {
                VariableId = HAS_EXTRACTION_OUTPUT_TARGET,
                Curve = UtilityCurveType.Linear,
                Weight = 1f
            }
        };

        private static float ReadHasExtractionOutputTarget(SW.Entity entity)
        {
            return AiBlackboardAccess.TryGetEntity(entity, HaulExtractionOutputCollectVariables.TargetExtractionBuilding, out _) ? 1f : 0f;
        }
    }
}
