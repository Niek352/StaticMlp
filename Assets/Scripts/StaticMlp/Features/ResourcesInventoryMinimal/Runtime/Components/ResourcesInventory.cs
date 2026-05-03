using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public struct ResourcesInventory : IComponent, ITrackableChanged
    {
        public int Wood;
        public int Stone;

        public int SpendWood(int requested)
        {
            var amount = Mathf.Clamp(requested, 0, Wood);
            Wood -= amount;
            return amount;
        }

        public int SpendStone(int requested)
        {
            var amount = Mathf.Clamp(requested, 0, Stone);
            Stone -= amount;
            return amount;
        }
    }
}
