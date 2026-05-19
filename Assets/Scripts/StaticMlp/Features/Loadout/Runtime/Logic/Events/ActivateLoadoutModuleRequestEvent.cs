using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public readonly struct ActivateLoadoutModuleRequestEvent : IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly LoadoutModuleId ModuleId;
        public readonly EquipmentSlotKind SlotKind;
        public readonly int SlotIndex;

        public ActivateLoadoutModuleRequestEvent(
            NetworkPeerId sourcePeer,
            LoadoutModuleId moduleId,
            EquipmentSlotKind slotKind,
            int slotIndex)
        {
            SourcePeer = sourcePeer;
            ModuleId = moduleId;
            SlotKind = slotKind;
            SlotIndex = slotIndex;
        }
    }
}
