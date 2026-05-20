namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingOperationDefinition
    {
        public static readonly BuildingOperationDefinition None = new(BuildingCapabilityFlags.None, 0, 0);

        public readonly BuildingCapabilityFlags OperationCapabilities;
        public readonly ushort StorageCapacity;
        public readonly byte WorkerSlots;

        public BuildingOperationDefinition(
            BuildingCapabilityFlags operationCapabilities,
            ushort storageCapacity,
            byte workerSlots)
        {
            OperationCapabilities = operationCapabilities;
            StorageCapacity = storageCapacity;
            WorkerSlots = workerSlots;
        }

        public bool IsDefined => OperationCapabilities != BuildingCapabilityFlags.None
                                 || StorageCapacity > 0
                                 || WorkerSlots > 0;
    }
}
