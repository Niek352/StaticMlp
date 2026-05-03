using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public struct ResourcesInventory : IComponent, ITrackableChanged
    {
        public int Wood;
        public int Stone;

        public int SpendWood(int requested)
        {
            var amount = Math.Min(Math.Max(0, requested), Wood);
            Wood -= amount;
            return amount;
        }

        public int SpendStone(int requested)
        {
            var amount = Math.Min(Math.Max(0, requested), Stone);
            Stone -= amount;
            return amount;
        }
    }
}
