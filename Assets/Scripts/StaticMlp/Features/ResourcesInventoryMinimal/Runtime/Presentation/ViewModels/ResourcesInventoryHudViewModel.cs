using System.Text;
using Aspid.MVVM;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [ViewModel]
    public sealed partial class ResourcesInventoryHudViewModel
    {
        [OneWayBind] private bool _isReady;
        [OneWayBind] private string _summary;

        public void Apply(in ResourcesInventoryHudViewData data)
        {
            var presentation = data.Presentation;
            IsReady = presentation.IsReady;
            Summary = BuildSummary(in presentation);
        }

        private static string BuildSummary(in ResourcesInventoryHudPresentation presentation)
        {
            if (!presentation.IsReady)
                return "Inventory: not ready";

            var builder = new StringBuilder();
            builder.Append("Inventory: ");
            builder.Append(presentation.UsedSlots);
            builder.Append(" / ");
            builder.Append(presentation.Capacity);
            builder.Append(" slots, ");
            builder.Append(presentation.TotalAmount);
            builder.Append(" total");

            for (var i = 0; i < presentation.Slots.Length; i++)
            {
                var slot = presentation.Slots[i];
                if (!slot.IsOccupied)
                    continue;

                builder.Append("\n");
                builder.Append(slot.ResourceName);
                builder.Append(": ");
                builder.Append(slot.Amount);
            }

            return builder.ToString();
        }
    }
}
