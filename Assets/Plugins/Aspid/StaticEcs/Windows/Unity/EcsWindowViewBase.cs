using System;
using Aspid.MVVM;

namespace Aspid.StaticEcs.Windows
{
    public abstract class EcsWindowViewBase<TSlot, TViewModel> : EcsWindowShellViewBase, IView<TViewModel>
        where TSlot : struct, IEcsWindowSlot
        where TViewModel : class, IViewModel
    {
        private TViewModel _viewModel;

        public IViewModel ViewModel => _viewModel;

        protected TViewModel BoundViewModel => _viewModel
            ?? throw new InvalidOperationException($"{GetType().Name} is not bound to a ViewModel.");

        protected override void Awake()
        {
            base.Awake();
            RegisterSlot<TSlot>(this);
        }

        public void Initialize(IViewModel viewModel)
        {
            if (viewModel is not TViewModel typedViewModel)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} requires ViewModel `{typeof(TViewModel).FullName}`, got `{viewModel?.GetType().FullName}`.");
            }

            Initialize(typedViewModel);
        }

        public void Initialize(TViewModel viewModel)
        {
            if (_viewModel != null)
                throw new InvalidOperationException($"{GetType().Name} is already bound to a ViewModel.");

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            OnViewModelBound(viewModel);
        }

        public void Deinitialize()
        {
            if (_viewModel == null)
                throw new InvalidOperationException($"{GetType().Name} is not bound to a ViewModel.");

            var viewModel = _viewModel;
            OnViewModelUnbound(viewModel);
            _viewModel = null;
        }

        protected virtual void OnViewModelBound(TViewModel viewModel)
        {
        }

        protected virtual void OnViewModelUnbound(TViewModel viewModel)
        {
        }
    }
}
