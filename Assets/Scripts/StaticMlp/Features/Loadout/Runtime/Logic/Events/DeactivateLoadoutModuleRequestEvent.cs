using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public readonly struct DeactivateLoadoutModuleRequestEvent : IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly EquipmentSlotKind SlotKind;
        public readonly int SlotIndex;

        public DeactivateLoadoutModuleRequestEvent(NetworkPeerId sourcePeer, EquipmentSlotKind slotKind, int slotIndex)
        {
            SourcePeer = sourcePeer;
            SlotKind = slotKind;
            SlotIndex = slotIndex;
        }
    }
}
