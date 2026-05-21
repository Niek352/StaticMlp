using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct BedrollShelterState : IComponent
    {
        public byte SlotCount;
        public bool Enabled;
    }
}
