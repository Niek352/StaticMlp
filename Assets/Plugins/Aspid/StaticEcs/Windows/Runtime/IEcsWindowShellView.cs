using System;
using System.Threading;
using Aspid.MVVM;
using Cysharp.Threading.Tasks;

namespace Aspid.StaticEcs.Windows
{
    public interface IEcsWindowShellView : IDisposable
    {
        void SetDrawOrder(EcsWindowOrdering order);

        UniTask ShowAsync(CancellationToken ct);

        UniTask HideAsync(CancellationToken ct, bool isInstant = false);

        void SetPresentationActive(bool isActive);

        void Focus();

        void Blur();

        void Bind(Type slotType, IViewModel viewModel);

        void Unbind(Type slotType);
    }
}
