using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.EcsUi.Mvc
{
    public abstract class PrefabViewBase : MonoBehaviour, IView
    {
        [SerializeField] private Canvas canvas; //Can be null by design
        [SerializeField] private GraphicRaycaster graphicRaycaster; //Can be null by design

        public event Action ViewHidden;
        public event Action ViewShown;

        protected Canvas Canvas => canvas;

        protected GraphicRaycaster GraphicRaycaster => graphicRaycaster;

        protected virtual void Awake()
        {
        }

        public void SetDrawOrder(ViewOrdering order)
        {
            if (canvas == null)
                return;

            canvas.sortingLayerName = PrefabSortingLayerNames.For(order.Layer);
            canvas.sortingOrder = order.OrderInLayer;
        }

        public virtual async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            SetRaycasterEnabled(false);
            await PlayShowAnimationAsync(ct);
            SetRaycasterEnabled(true);
            ViewShown?.Invoke();
        }

        public virtual async UniTask HideAsync(CancellationToken ct, bool isInstant = false)
        {
            SetRaycasterEnabled(false);

            if (!isInstant)
                await PlayHideAnimationAsync(ct);

            gameObject.SetActive(false);
            ViewHidden?.Invoke();
        }

        public virtual void SetPresentationActive(bool isActive)
        {
            canvas.enabled = isActive;
            SetRaycasterEnabled(isActive);
        }

        public virtual void Dispose()
        {
            if (this == null)
                return;

            Destroy(gameObject);
        }

        protected virtual UniTask PlayShowAnimationAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask PlayHideAnimationAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        private void SetRaycasterEnabled(bool isEnabled)
        {
            if (graphicRaycaster == null)
                return;

            graphicRaycaster.enabled = isEnabled
                && (canvas == null || canvas.enabled)
                && gameObject.activeInHierarchy;
        }

        private void Reset()
        {
#if UNITY_EDITOR
            name = GetType().Name;
#endif
        }
    }
}
