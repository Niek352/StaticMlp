using System.Text;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Settlement;
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
            resourcesLabel.text = FormatResources(in component);
        }

        private static string FormatResources(in ConstructionViewState component)
        {
            if (component.Resources.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();
            for (var i = 0; i < component.Resources.Length; i++)
            {
                if (i > 0)
                    builder.Append("  ");

                var resource = component.Resources[i];
                builder.Append(ResourceCatalog.Get(resource.Id).DisplayName);
                builder.Append(' ');
                builder.Append(resource.Delivered);
                builder.Append('/');
                builder.Append(resource.Required);
            }

            return builder.ToString();
        }
    }
}
