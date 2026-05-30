using System;
using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs
{
    public sealed class EcsLink<TWorld, TViewModel> : IDisposable
        where TWorld : struct, IWorldType
        where TViewModel : class, IViewModel
    {
        private readonly EcsLinkRegistry<TWorld> _registry;
        private readonly bool _disposeViewModel;

        internal EcsLink(
            EcsLinkRegistry<TWorld> registry,
            EntityGID entityGID,
            TViewModel viewModel,
            bool disposeViewModel)
        {
            _registry = registry;
            EntityGID = entityGID;
            ViewModel = viewModel;
            _disposeViewModel = disposeViewModel;
        }

        public EntityGID EntityGID { get; }

        public TViewModel ViewModel { get; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            _registry.Remove(this);
        }

        internal void DisposeFromRegistry()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (_disposeViewModel && ViewModel is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
