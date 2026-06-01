using Aspid.MVVM;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    [View]
    public sealed partial class InteractionPromptView : MonoView, IView<InteractionPromptViewModel>
    {
        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _isVisible;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;
    }
}
