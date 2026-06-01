using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

namespace StaticMlp.Features.Progression
{
    [View]
    public sealed partial class RewardResultPopupView : MonoView, IView<RewardResultPopupViewModel>
    {
        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;

        [SerializeField] private ButtonCommandBinder[] _closePopupCommand;
    }
}
