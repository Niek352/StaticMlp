using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    [View]
    public sealed partial class SettlementContextPanelView : MonoView, IView<SettlementContextPanelViewModel>
    {
        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _primaryActionLabel;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _secondaryActionLabel;

        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _secondaryActionVisible;

        [SerializeField] private ButtonCommandBinder[] _primaryActionCommand;
        [SerializeField] private ButtonCommandBinder[] _secondaryActionCommand;
    }
}
