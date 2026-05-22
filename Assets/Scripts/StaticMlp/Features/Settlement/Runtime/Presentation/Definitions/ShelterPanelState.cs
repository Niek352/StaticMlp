using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct ShelterPanelState
    {
        public EntityGID Target;
        public string DisplayName;
        public byte SlotCount;
        public byte FreeSlots;
        public bool Enabled;
    }
}
