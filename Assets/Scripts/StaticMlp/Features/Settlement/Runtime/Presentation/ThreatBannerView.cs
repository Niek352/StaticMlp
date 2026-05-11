using Code.EcsUi.Mvc;
using StaticMlp.Features.Frontier;
using TMPro;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public sealed class ThreatBannerView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(ThreatBannerView)} requires {nameof(panelRoot)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(ThreatBannerView)} requires {nameof(summaryLabel)}.");
        }

        public void Render(in ThreatBannerState state)
        {
            panelRoot.SetActive(state.IsVisible);
            summaryLabel.text =
                $"Threat: {state.Phase} | Raid: {state.RaidStatus} | ActivateAtTick: {state.ActivateAtTick}";
        }
    }
}
