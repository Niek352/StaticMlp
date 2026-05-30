using System;
using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Examples
{
    [ViewModel]
    public sealed partial class ExampleUnitViewModel : IDisposable
    {
        [OneWayBind] private int _healthCurrent;
        [OneWayBind] private int _healthMax;
        [OneWayBind] private bool _isSelected;
        [OneWayBind] private int _inventorySlots;
        [OneWayBind] private int _inventoryTotal;

        public ExampleUnitViewModel(EntityGID entityGID)
        {
            EntityGID = entityGID;
        }

        public EntityGID EntityGID { get; }
        public bool IsDisposed { get; private set; }

        public void Apply(in ExampleUnitHealth health)
        {
            HealthCurrent = health.Current;
            HealthMax = health.Max;
        }

        public void ApplySelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        public void ApplyInventory(in World<ExampleWorld>.Multi<ExampleInventoryItem> inventory)
        {
            var total = 0;

            for (var i = 0; i < inventory.Length; i++)
            {
                ref readonly var item = ref inventory.Get(i);
                total += item.Count;
            }

            InventorySlots = inventory.Length;
            InventoryTotal = total;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
