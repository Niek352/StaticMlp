using FFS.Libraries.StaticEcs;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct StockpilePanelState
    {
        public EntityGID Target;
        public string DisplayName;
        public FixedList512Bytes<SettlementResourceViewEntry> Resources;
        public int UsedCapacity;
        public int Capacity;
        public int RemainingCapacity;
        public bool CarriedRawInventoryReady;
        public int CarriedRawAmount;
        public int ExpectedDepositAmount;
        public ushort ContributedCapacity;
    }
}
