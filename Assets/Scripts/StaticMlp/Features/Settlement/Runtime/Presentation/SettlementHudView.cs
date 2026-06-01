using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    [View]
    public sealed partial class SettlementHudView : MonoView, IView<SettlementHudViewModel>
    {
        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;

        [SerializeField] private ButtonCommandBinder[] _openLoadoutPreparationCommand;
        [SerializeField] private ButtonCommandBinder[] _openExpeditionSelectionCommand;
        [SerializeField] private ButtonCommandBinder[] _closeHudCommand;
    }
}
