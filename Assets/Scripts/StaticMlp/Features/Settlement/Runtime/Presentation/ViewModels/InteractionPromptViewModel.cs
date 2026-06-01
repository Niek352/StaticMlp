using Aspid.MVVM;

namespace StaticMlp.Features.Settlement
{
    [ViewModel]
    public sealed partial class InteractionPromptViewModel
    {
        [OneWayBind] private bool _isVisible;
        [OneWayBind] private string _summary;

        public void Apply(in InteractionPromptViewData data)
        {
            var state = data.State;
            IsVisible = state.IsVisible;
            Summary = state.IsVisible
                ? $"{state.InputHint}: {state.PromptLabel}\n{state.EffectDescription}"
                : string.Empty;
        }
    }
}
