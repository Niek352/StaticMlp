using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    [View]
    public sealed partial class ExpeditionSelectionView : MonoView, IView<ExpeditionSelectionViewModel>
    {
        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;

        [SerializeField] private ButtonCommandBinder[] _startExpeditionCommand;
        [SerializeField] private ButtonCommandBinder[] _closeCommand;
    }
}
