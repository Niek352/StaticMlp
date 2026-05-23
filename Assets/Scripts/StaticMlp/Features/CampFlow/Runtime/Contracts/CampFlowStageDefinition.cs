namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowStageDefinition
    {
        public readonly CampFlowStage Stage;
        public readonly string ObjectiveDisplayName;
        public readonly string HintDisplayName;
        public readonly bool AutoAdvance;

        public CampFlowStageDefinition(
            CampFlowStage stage,
            string objectiveDisplayName,
            string hintDisplayName,
            bool autoAdvance)
        {
            Stage = stage;
            ObjectiveDisplayName = objectiveDisplayName;
            HintDisplayName = hintDisplayName;
            AutoAdvance = autoAdvance;
        }
    }
}
