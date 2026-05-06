using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public interface IMvcManager : IDisposable
    {
        event Action<IController> OnViewShown;

        event Action<IController> OnViewClosed;

        UniTask ShowAsync<TView, TInputData>(ShowCommand<TView, TInputData> command, CancellationToken ct = default)
            where TView : IView;

        void RegisterController<TView, TInputData>(IController<TView, TInputData> controller)
            where TView : IView;

        bool TryCloseTopClosable();

        bool TryClose(IController controller);

        void SetAllViewsPresentationActive(bool isActive);

        void SetAllViewsPresentationActive(IController except, bool isActive);
    }
}
