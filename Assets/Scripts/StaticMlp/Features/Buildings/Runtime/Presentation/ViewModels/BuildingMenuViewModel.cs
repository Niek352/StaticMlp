using System;
using System.Text;
using Aspid.MVVM;
using Aspid.Collections.Observable;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    [ViewModel]
    public sealed partial class BuildingMenuViewModel
    {
        [OneWayBind] private bool _isOpen;
        [OneWayBind] private string _selectedBuildingName;
        [OneWayBind] private string _summary;
        [OneTimeBind] private readonly ObservableList<BuildingMenuCardViewModel> _cards = new();

        public void Apply(in BuildingMenuViewData data)
        {
            var presentation = data.Presentation;
            IsOpen = presentation.IsOpen;
            SelectedBuildingName = presentation.SelectedBuildingName;
            Summary = BuildSummary(in presentation);

            ApplyCards(presentation.Cards ?? Array.Empty<BuildingMenuCardPresentation>());
        }

        [RelayCommand]
        private void Close()
        {
            CW.SendEvent(new BuildingMenuCloseIntent());
        }

        private void ApplyCards(BuildingMenuCardPresentation[] cards)
        {
            while (Cards.Count > cards.Length)
                Cards.RemoveAt(Cards.Count - 1);

            while (Cards.Count < cards.Length)
                Cards.Add(new BuildingMenuCardViewModel());

            for (var i = 0; i < cards.Length; i++)
                Cards[i].Apply(in cards[i]);
        }

        private static string BuildSummary(in BuildingMenuPresentation presentation)
        {
            var builder = new StringBuilder();
            builder.Append("Selected: ");
            builder.Append(presentation.SelectedBuildingName);

            if (presentation.Categories is { Length: > 0 })
            {
                builder.Append("\nCategories: ");
                for (var i = 0; i < presentation.Categories.Length; i++)
                {
                    if (i > 0)
                        builder.Append(" / ");

                    var category = presentation.Categories[i];
                    if (category.IsSelected)
                        builder.Append("> ");

                    builder.Append(category.Label);
                    builder.Append(" (");
                    builder.Append(category.CardCount);
                    builder.Append(')');
                }
            }

            return builder.ToString();
        }
    }
}
