using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public abstract class ControllerBase<TView> : ControllerBase<TView, ControllerNoData>
        where TView : IView
    {
        protected ControllerBase(ViewFactoryMethod<TView> viewFactory)
            : base(viewFactory)
        {
        }

        public static ShowCommand<TView, ControllerNoData> IssueCommand()
        {
            return new ShowCommand<TView, ControllerNoData>(default);
        }
    }

    public abstract class ControllerBase<TView, TInputData> : IController<TView, TInputData>
        where TView : IView
    {
        private readonly ViewFactoryMethod<TView> viewFactory;
        private readonly List<IMvcControllerModule> modules = new();
        private UniTaskCompletionSource closeIntent;

        protected ControllerBase(ViewFactoryMethod<TView> viewFactory)
        {
            this.viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            State = ControllerState.ViewHidden;
        }

        protected TView View { get; private set; }

        protected TInputData InputData { get; private set; }

        public ControllerState State { get; private set; }

        public abstract ViewLayer Layer { get; }

        public virtual int? PersistentSortOrder => null;

        public virtual bool CanBeClosedByEscape => Layer is ViewLayer.Popup or ViewLayer.Fullscreen;

        public static ShowCommand<TView, TInputData> IssueCommand(TInputData inputData)
        {
            return new ShowCommand<TView, TInputData>(inputData);
        }

        protected TModule AddModule<TModule>(TModule module)
            where TModule : class, IMvcControllerModule
        {
            modules.Add(module);
            return module;
        }

        public async UniTask LaunchViewLifeCycleAsync(ViewOrdering ordering, TInputData data, CancellationToken ct)
        {
            var isNewView = View == null;
            View ??= viewFactory();

            if (isNewView)
                OnViewInstantiated();

            InputData = data;
            closeIntent = new UniTaskCompletionSource();

            View.SetDrawOrder(ordering);
            OnBeforeViewShow();

            State = ControllerState.ViewShowing;
            await View.ShowAsync(ct);

            State = ControllerState.ViewFocused;
            OnViewShow();

            for (var i = 0; i < modules.Count; i++)
                modules[i].OnViewShow();

            await WaitForCloseIntentAsync(ct);
        }

        public async UniTask HideViewAsync(CancellationToken ct)
        {
            State = ControllerState.ViewHiding;

            for (var i = 0; i < modules.Count; i++)
                modules[i].OnViewHide();

            OnViewClose();

            if (View != null)
                await View.HideAsync(ct);

            State = ControllerState.ViewHidden;
        }

        public void SetViewPresentationActive(bool isActive)
        {
            View?.SetPresentationActive(isActive);
        }

        public void Focus()
        {
            if (State == ControllerState.ViewHidden)
                return;

            State = ControllerState.ViewFocused;

            for (var i = 0; i < modules.Count; i++)
                modules[i].OnFocus();

            OnFocus();
        }

        public void Blur()
        {
            if (State == ControllerState.ViewHidden)
                return;

            State = ControllerState.ViewBlurred;

            for (var i = 0; i < modules.Count; i++)
                modules[i].OnBlur();

            OnBlur();
        }

        public void RequestClose()
        {
            closeIntent?.TrySetResult();
        }

        protected virtual void OnViewInstantiated()
        {
        }

        protected virtual void OnFocus()
        {
        }

        protected virtual void OnBlur()
        {
        }

        protected virtual void OnBeforeViewShow()
        {
        }

        protected virtual void OnViewShow()
        {
        }

        protected virtual void OnViewClose()
        {
        }

        protected virtual UniTask WaitForCloseIntentAsync(CancellationToken ct)
        {
            return closeIntent != null
                ? closeIntent.Task.AttachExternalCancellation(ct)
                : UniTask.Never(ct);
        }

        public virtual void Dispose()
        {
            View?.Dispose();
        }
    }
}
