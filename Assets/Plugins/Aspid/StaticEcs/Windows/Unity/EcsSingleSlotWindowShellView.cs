using System;
using Aspid.MVVM;
using UnityEngine;

namespace Aspid.StaticEcs.Windows
{
    public abstract class EcsSingleSlotWindowShellView<TSlot, TViewModel> : EcsWindowShellViewBase
        where TSlot : struct, IEcsWindowSlot
        where TViewModel : class, IViewModel
    {
        [SerializeField] private MonoBehaviour _slotView;

        protected override void Awake()
        {
            base.Awake();

            if (_slotView == null)
                throw new MissingReferenceException($"{GetType().Name} requires {nameof(_slotView)}.");

            if (_slotView is not IView<TViewModel> view)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} requires {nameof(_slotView)} implementing `{typeof(IView<TViewModel>).FullName}`.");
            }

            RegisterSlot<TSlot>(view);
        }
    }
}
