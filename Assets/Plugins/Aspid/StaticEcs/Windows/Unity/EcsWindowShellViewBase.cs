using System;
using System.Collections.Generic;
using System.Threading;
using Aspid.MVVM;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Aspid.StaticEcs.Windows
{
    public abstract class EcsWindowShellViewBase : MonoBehaviour, IEcsWindowShellView
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        private readonly Dictionary<Type, SlotBinding> _slotsByType = new();
        private bool _isFocused;
        private bool _isShown;
        private bool _isPresentationActive = true;

        protected Canvas Canvas => _canvas;

        protected CanvasGroup CanvasGroup => _canvasGroup;

        protected GraphicRaycaster GraphicRaycaster => _graphicRaycaster;

        protected virtual void Awake()
        {
            if (_canvas == null)
                throw new InvalidOperationException($"{GetType().Name} requires a {nameof(Canvas)} reference.");

            if (_canvasGroup == null)
                throw new InvalidOperationException($"{GetType().Name} requires a {nameof(CanvasGroup)} reference.");

            if (_graphicRaycaster == null)
                throw new InvalidOperationException($"{GetType().Name} requires a {nameof(GraphicRaycaster)} reference.");
        }

        public void SetDrawOrder(EcsWindowOrdering order)
        {
            _canvas.sortingLayerName = EcsWindowSortingLayerNames.For(order.Layer);
            _canvas.sortingOrder = order.OrderInLayer;
        }

        public virtual async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            _isShown = true;
            _canvasGroup.alpha = 1f;
            SetRaycasterEnabled(false);
            await PlayShowAnimationAsync(ct);
            SetRaycasterEnabled(_isFocused);
        }

        public virtual async UniTask HideAsync(CancellationToken ct, bool isInstant = false)
        {
            Blur();

            if (!isInstant)
                await PlayHideAnimationAsync(ct);

            _canvasGroup.alpha = 0f;
            _isShown = false;
            gameObject.SetActive(false);
        }

        public virtual void SetPresentationActive(bool isActive)
        {
            _isPresentationActive = isActive;
            _canvas.enabled = isActive;
            SetRaycasterEnabled(_isFocused);
        }

        public virtual void Focus()
        {
            _isFocused = true;
            SetRaycasterEnabled(true);
        }

        public virtual void Blur()
        {
            _isFocused = false;
            SetRaycasterEnabled(false);
        }

        public void Bind(Type slotType, IViewModel viewModel)
        {
            if (slotType == null)
                throw new ArgumentNullException(nameof(slotType));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var slot = GetSlot(slotType);
            if (slot.IsBound)
                throw new InvalidOperationException($"{GetType().Name} slot `{slotType.FullName}` is already bound.");

            if (slot.View.ViewModel != null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} slot `{slotType.FullName}` child view is already initialized.");
            }

            slot.View.Initialize(viewModel);
            slot.IsBound = true;
        }

        public void Unbind(Type slotType)
        {
            if (slotType == null)
                throw new ArgumentNullException(nameof(slotType));

            var slot = GetSlot(slotType);
            if (!slot.IsBound)
                throw new InvalidOperationException($"{GetType().Name} slot `{slotType.FullName}` is not bound.");

            slot.View.Deinitialize();
            slot.IsBound = false;
        }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }

        protected void RegisterSlot<TSlot>(IView view)
            where TSlot : struct, IEcsWindowSlot
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var slotType = typeof(TSlot);
            if (_slotsByType.ContainsKey(slotType))
                throw new InvalidOperationException($"{GetType().Name} already registered slot `{slotType.FullName}`.");

            _slotsByType.Add(slotType, new SlotBinding(view));
        }

        protected virtual UniTask PlayShowAnimationAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask PlayHideAnimationAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        private SlotBinding GetSlot(Type slotType)
        {
            if (_slotsByType.TryGetValue(slotType, out var slot))
                return slot;

            throw new InvalidOperationException($"{GetType().Name} has no child Aspid view registered for slot `{slotType.FullName}`.");
        }

        private void SetRaycasterEnabled(bool isEnabled)
        {
            _graphicRaycaster.enabled = isEnabled
                && _isPresentationActive
                && _isShown
                && _canvas.enabled
                && gameObject.activeInHierarchy;
        }

        private sealed class SlotBinding
        {
            public readonly IView View;
            public bool IsBound;

            public SlotBinding(IView view)
            {
                View = view;
            }
        }
    }
}
