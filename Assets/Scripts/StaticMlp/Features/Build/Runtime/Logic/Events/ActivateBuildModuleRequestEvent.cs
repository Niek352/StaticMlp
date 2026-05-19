using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public readonly struct ActivateBuildModuleRequestEvent : IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly BuildModuleId ModuleId;
        public readonly EquipmentSlotKind SlotKind;
        public readonly int SlotIndex;

        public ActivateBuildModuleRequestEvent(
            NetworkPeerId sourcePeer,
            BuildModuleId moduleId,
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
