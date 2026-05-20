namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingNpcProfileDefinition
    {
        public static readonly BuildingNpcProfileDefinition None = new(false, 0);

        public readonly bool SupportsWorkers;
        public readonly byte WorkerSlots;

        public BuildingNpcProfileDefinition(bool supportsWorkers, byte workerSlots)
        {
            SupportsWorkers = supportsWorkers;
            WorkerSlots = workerSlots;
        }

        public bool IsDefined => SupportsWorkers || WorkerSlots > 0;
    }
}
