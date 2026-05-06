using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public interface IController : IDisposable
    {
        ControllerState State { get; }

        ViewLayer Layer { get; }

        int? PersistentSortOrder { get; }

        bool CanBeClosedByEscape { get; }

        void Focus();

        void Blur();

        UniTask HideViewAsync(CancellationToken ct);

        void SetViewPresentationActive(bool isActive);

        void RequestClose();
    }

    public interface IController<TView, in TInputData> : IController
        where TView : IView
    {
        UniTask LaunchViewLifeCycleAsync(ViewOrdering ordering, TInputData data, CancellationToken ct);
    }
}
