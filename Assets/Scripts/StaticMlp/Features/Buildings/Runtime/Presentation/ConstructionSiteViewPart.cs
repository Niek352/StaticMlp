using StaticMlp.Features.EcsViews;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Buildings
{
    public sealed class ConstructionSiteViewPart : MonoBehaviour, IEntityViewPart<ConstructionViewState>
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI phaseLabel;
        [SerializeField] private TextMeshProUGUI resourcesLabel;

        private void Awake()
        {
            if (progressSlider == null)
                throw new MissingReferenceException($"{nameof(ConstructionSiteViewPart)} requires {nameof(progressSlider)}.");
            if (phaseLabel == null)
                throw new MissingReferenceException($"{nameof(ConstructionSiteViewPart)} requires {nameof(phaseLabel)}.");
            if (resourcesLabel == null)
                throw new MissingReferenceException($"{nameof(ConstructionSiteViewPart)} requires {nameof(resourcesLabel)}.");
        }

        public void OnBind(IEntityView view)
        {
            if (view.Entity.Has<ConstructionViewState>())
            {
                ref readonly var state = ref view.Entity.Read<ConstructionViewState>();
                Apply(in state);
            }
        }

        public void OnUnbind()
        {
        }

        public void Apply(in ConstructionViewState component)
        {
            progressSlider.value = component.Progress01;
            phaseLabel.text = component.Phase.ToString();
            resourcesLabel.text =
                $"Wood {component.WoodDelivered}/{component.WoodRequired}  Stone {component.StoneDelivered}/{component.StoneRequired}";
        }
    }
}
