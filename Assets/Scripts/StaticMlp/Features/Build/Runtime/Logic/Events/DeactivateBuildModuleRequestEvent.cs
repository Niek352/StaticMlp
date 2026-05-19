using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public readonly struct DeactivateBuildModuleRequestEvent : IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly EquipmentSlotKind SlotKind;
        public readonly int SlotIndex;

        public DeactivateBuildModuleRequestEvent(NetworkPeerId sourcePeer, EquipmentSlotKind slotKind, int slotIndex)
        {
            SourcePeer = sourcePeer;
            SlotKind = slotKind;
            SlotIndex = slotIndex;
        }
    }
}
