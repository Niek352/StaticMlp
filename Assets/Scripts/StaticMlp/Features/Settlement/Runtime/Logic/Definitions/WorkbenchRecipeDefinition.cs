namespace StaticMlp.Features.Settlement
{
    public readonly struct WorkbenchRecipeDefinition
    {
        public readonly WorkbenchRecipeId Id;
        public readonly string Code;
        public readonly ResourceAmount[] Inputs;
        public readonly ResourceAmount[] Outputs;
        public readonly float WorkRequired;

        public WorkbenchRecipeDefinition(
            WorkbenchRecipeId id,
            string code,
            ResourceAmount[] inputs,
            ResourceAmount[] outputs,
            float workRequired)
        {
            Id = id;
            Code = code;
            Inputs = inputs;
            Outputs = outputs;
            WorkRequired = workRequired;
        }
    }
}
