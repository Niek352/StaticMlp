using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Loadout
{
    [View]
    public sealed partial class LoadoutPreparationView : MonoView, IView<LoadoutPreparationViewModel>
    {
        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _selectedPrimaryModuleName;

        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _poisonArrowSelected;

        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _fireFlaskSelected;

        [SerializeField] private ButtonCommandBinder[] _selectPoisonArrowCommand;
        [SerializeField] private ButtonCommandBinder[] _selectFireFlaskCommand;
        [SerializeField] private ButtonCommandBinder[] _confirmBuildCommand;
        [SerializeField] private ButtonCommandBinder[] _closeCommand;
    }
}
