using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    [View]
    public sealed partial class BuildingMenuCardView : MonoView, IView<BuildingMenuCardViewModel>
    {
        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _displayName;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _categoryLabel;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _costLabel;

        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _isSelected;

        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _isAvailable;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _lockedReason;

        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _lockedReasonVisible;

        [SerializeField] private ButtonCommandMonoBinder[] _selectCommand;
    }
}
