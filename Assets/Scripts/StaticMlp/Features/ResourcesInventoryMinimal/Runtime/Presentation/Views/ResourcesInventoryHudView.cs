using System;
using Aspid.StaticEcs.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudView : EcsWindowViewBase<ResourcesInventoryHudSlot, ResourcesInventoryHudViewModel>
    {
        private static readonly Color COLOR_OCCUPIED = new(0.24f, 0.36f, 0.31f, 0.95f);
        private static readonly Color COLOR_EMPTY = new(0.11f, 0.16f, 0.18f, 0.82f);

        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _summaryLabel;
        [SerializeField] private GameObject[] _slotRoots;
        [SerializeField] private Image[] _slotBackgrounds;
        [SerializeField] private TextMeshProUGUI[] _slotNameLabels;
        [SerializeField] private TextMeshProUGUI[] _slotAmountLabels;

        protected override void Awake()
        {
            base.Awake();

            if (_panelRoot == null)
                throw new MissingReferenceException($"{nameof(ResourcesInventoryHudView)} requires {nameof(_panelRoot)}.");
            if (_summaryLabel == null)
                throw new MissingReferenceException($"{nameof(ResourcesInventoryHudView)} requires {nameof(_summaryLabel)}.");

            CheckSlotBindings();
        }

        protected override void OnViewModelBound(ResourcesInventoryHudViewModel viewModel)
        {
            viewModel.Changed += Render;
            Render();
        }

        protected override void OnViewModelUnbound(ResourcesInventoryHudViewModel viewModel)
        {
            viewModel.Changed -= Render;
        }

        private void Render()
        {
            var presentation = BoundViewModel.Presentation;
            Render(in presentation);
        }

        public void Render(in ResourcesInventoryHudPresentation presentation)
        {
            _panelRoot.SetActive(presentation.IsReady);
            if (!presentation.IsReady)
                return;

            if (presentation.Slots.Length != ResourcesInventory.MAX_SLOTS)
                throw new InvalidOperationException(
                    $"{nameof(ResourcesInventoryHudView)} expected {ResourcesInventory.MAX_SLOTS} inventory slots, got {presentation.Slots.Length}.");

            _summaryLabel.text = $"Slots {presentation.UsedSlots}/{presentation.Capacity}";

            for (var i = 0; i < presentation.Slots.Length; i++)
                RenderSlot(i, in presentation.Slots[i]);
        }

        private void RenderSlot(int index, in ResourcesInventorySlotPresentation slot)
        {
            _slotRoots[index].SetActive(true);
            _slotBackgrounds[index].color = slot.IsOccupied ? COLOR_OCCUPIED : COLOR_EMPTY;
            _slotNameLabels[index].text = slot.IsOccupied ? slot.ResourceName : "Empty";
            _slotAmountLabels[index].text = slot.IsOccupied ? slot.Amount.ToString() : string.Empty;
        }

        private void CheckSlotBindings()
        {
            CheckArray(_slotRoots, nameof(_slotRoots));
            CheckArray(_slotBackgrounds, nameof(_slotBackgrounds));
            CheckArray(_slotNameLabels, nameof(_slotNameLabels));
            CheckArray(_slotAmountLabels, nameof(_slotAmountLabels));
        }

        private static void CheckArray<T>(T[] bindings, string fieldName)
            where T : UnityEngine.Object
        {
            if (bindings == null || bindings.Length != ResourcesInventory.MAX_SLOTS)
                throw new MissingReferenceException(
                    $"{nameof(ResourcesInventoryHudView)} requires {ResourcesInventory.MAX_SLOTS} entries in {fieldName}.");

            for (var i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] == null)
                    throw new MissingReferenceException($"{nameof(ResourcesInventoryHudView)} requires {fieldName}[{i}].");
            }
        }
    }
}
