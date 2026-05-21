using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct ExtractionOperationState : IComponent
    {
        public ushort OutputResourceId;
        public int OutputBufferAmount;
        public ushort OutputBufferCapacity;
        public bool Enabled;
        public byte WorkerSlotCount;

        public ResourceId OutputResource => new(OutputResourceId);
    }
}
