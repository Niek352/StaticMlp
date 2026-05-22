using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct BuildingOperationDefinition
    {
        public static readonly BuildingOperationDefinition None = new(BuildingCapabilityFlags.None, 0, 0);

        public readonly BuildingCapabilityFlags OperationCapabilities;
        public readonly ushort StorageCapacity;
        public readonly byte WorkerSlots;
        public readonly ResourceId OutputResourceId;

        public BuildingOperationDefinition(
            BuildingCapabilityFlags operationCapabilities,
            ushort storageCapacity,
            byte workerSlots,
            ResourceId outputResourceId = default)
        {
            OperationCapabilities = operationCapabilities;
            StorageCapacity = storageCapacity;
            WorkerSlots = workerSlots;
            OutputResourceId = outputResourceId;
        }

        public bool IsDefined => OperationCapabilities != BuildingCapabilityFlags.None
                                 || StorageCapacity > 0
                                 || WorkerSlots > 0
                                 || OutputResourceId.Value != 0;
    }
}
