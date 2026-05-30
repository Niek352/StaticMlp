using System;
using Aspid.MVVM;

namespace StaticMlp.Features.Settlement
{
    [ViewModel]
    public sealed partial class InteractionPromptViewModel
    {
        [OneWayBind] private InteractionPromptState _state;

        public event Action Changed;

        public void Apply(in InteractionPromptViewData data)
        {
            State = data.State;
        }

        partial void OnStateChanged(InteractionPromptState newValue)
        {
            Changed?.Invoke();
        }
    }
}
