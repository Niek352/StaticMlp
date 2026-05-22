using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Interaction;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct InteractionPromptState : IResource
    {
        public bool IsVisible;
        public EntityGID Target;
        public InteractableKind Kind;
        public string InputHint;
        public string PromptLabel;
        public string EffectDescription;

        public void Hide()
        {
            IsVisible = false;
            Target = default;
            Kind = InteractableKind.None;
            InputHint = string.Empty;
            PromptLabel = string.Empty;
            EffectDescription = string.Empty;
        }

        public void Show(
            EntityGID target,
            InteractableKind kind,
            string inputHint,
            string promptLabel,
            string effectDescription)
        {
            IsVisible = true;
            Target = target;
            Kind = kind;
            InputHint = inputHint;
            PromptLabel = promptLabel;
            EffectDescription = effectDescription;
        }
    }
}
