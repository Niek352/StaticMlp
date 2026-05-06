using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public interface IView : IDisposable
    {
        void SetDrawOrder(ViewOrdering order);

        UniTask ShowAsync(CancellationToken ct);

        UniTask HideAsync(CancellationToken ct, bool isInstant = false);

        void SetPresentationActive(bool isActive);
    }
}
