using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public sealed class InteractionPromptView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI promptLabel;
        [SerializeField] private TextMeshProUGUI effectLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(InteractionPromptView)} requires {nameof(panelRoot)}.");
            if (promptLabel == null)
                throw new MissingReferenceException($"{nameof(InteractionPromptView)} requires {nameof(promptLabel)}.");
            if (effectLabel == null)
                throw new MissingReferenceException($"{nameof(InteractionPromptView)} requires {nameof(effectLabel)}.");
        }

        public void Render(in InteractionPromptState state)
        {
            panelRoot.SetActive(state.IsVisible);
            if (!state.IsVisible)
                return;

            promptLabel.text = $"Press {state.InputHint} to {state.PromptLabel}";
            effectLabel.text = state.EffectDescription;
        }
    }
}
