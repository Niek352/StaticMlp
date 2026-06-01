using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    [View]
    public sealed partial class BuildingMenuView : MonoView, IView<BuildingMenuViewModel>
    {
        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _isOpen;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _selectedBuildingName;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;

        [SerializeField] private ButtonCommandBinder[] _closeMenuCommand;
        [SerializeField] private ButtonCommandBinder<int>[] _selectCardCommand;
    }
}
