namespace StaticMlp.Features.Settlement
{
    public readonly struct ProductionRecipeDefinition
    {
        public readonly ProductionStationId StationId;
        public readonly ProductionRecipeId Id;
        public readonly string Code;
        public readonly ResourceAmount[] Inputs;
        public readonly ResourceAmount[] Outputs;
        public readonly float WorkRequired;
        public readonly ResourceAmount? FuelRequirement;

        public ProductionRecipeDefinition(
            ProductionStationId stationId,
            ProductionRecipeId id,
            string code,
            ResourceAmount[] inputs,
            ResourceAmount[] outputs,
            float workRequired,
            ResourceAmount? fuelRequirement = null)
        {
            StationId = stationId;
            Id = id;
            Code = code;
            Inputs = inputs;
            Outputs = outputs;
            WorkRequired = workRequired;
            FuelRequirement = fuelRequirement;
        }
    }
}
