using System;
using System.Text;
using Aspid.MVVM;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    [ViewModel]
    public sealed partial class BuildingMenuViewModel
    {
        [OneWayBind] private bool _isOpen;
        [OneWayBind] private string _selectedBuildingName;
        [OneWayBind] private string _summary;

        private BuildingMenuCardPresentation[] _cards = Array.Empty<BuildingMenuCardPresentation>();

        public void Apply(in BuildingMenuViewData data)
        {
            var presentation = data.Presentation;
            _cards = presentation.Cards ?? Array.Empty<BuildingMenuCardPresentation>();
            IsOpen = presentation.IsOpen;
            SelectedBuildingName = presentation.SelectedBuildingName;
            Summary = BuildSummary(in presentation);

            SelectCardCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void CloseMenu()
        {
            CW.SendEvent(new BuildingMenuCloseIntent());
        }

        [RelayCommand(CanExecute = nameof(CanSelectCard))]
        private void SelectCard(int cardIndex)
        {
            var card = GetCard(cardIndex);
            CW.SendEvent(new BuildingMenuSelectIntent(card.BuildingId));
        }

        private bool CanSelectCard(int cardIndex)
        {
            return cardIndex >= 0
                   && cardIndex < _cards.Length
                   && _cards[cardIndex].IsAvailable;
        }

        private BuildingMenuCardPresentation GetCard(int cardIndex)
        {
            if (!CanSelectCard(cardIndex))
                throw new InvalidOperationException($"{nameof(BuildingMenuViewModel)} cannot select card slot {cardIndex}.");

            return _cards[cardIndex];
        }

        private static string BuildSummary(in BuildingMenuPresentation presentation)
        {
            var builder = new StringBuilder();
            builder.Append("Selected: ");
            builder.Append(presentation.SelectedBuildingName);

            if (presentation.Categories != null && presentation.Categories.Length > 0)
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

            if (presentation.Cards == null)
                return builder.ToString();

            for (var i = 0; i < presentation.Cards.Length; i++)
            {
                var card = presentation.Cards[i];
                builder.Append("\n");
                builder.Append(i);
                builder.Append(": ");
                if (card.IsSelected)
                    builder.Append("> ");

                builder.Append(card.DisplayName);
                builder.Append(" [");
                builder.Append(card.CategoryLabel);
                builder.Append("] ");
                builder.Append(card.IsAvailable ? card.CostLabel : $"Locked: {card.LockedReason}");
            }

            return builder.ToString();
        }
    }
}
