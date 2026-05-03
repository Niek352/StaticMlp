using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class PlacementPreviewViewPart : MonoBehaviour, IEntityViewPart<PlacementPreviewViewState>
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                throw new MissingReferenceException($"{nameof(PlacementPreviewViewPart)} requires renderers.");
            if (validMaterial == null)
                throw new MissingReferenceException($"{nameof(PlacementPreviewViewPart)} requires {nameof(validMaterial)}.");
            if (invalidMaterial == null)
                throw new MissingReferenceException($"{nameof(PlacementPreviewViewPart)} requires {nameof(invalidMaterial)}.");
        }

        public void OnBind(IEntityView view)
        {
            if (view.Entity.Has<PlacementPreviewViewState>())
            {
                ref readonly var state = ref view.Entity.Read<PlacementPreviewViewState>();
                Apply(in state);
            }
        }

        public void OnUnbind()
        {
        }

        public void Apply(in PlacementPreviewViewState component)
        {
            var material = component.IsValid ? validMaterial : invalidMaterial;
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].sharedMaterial = material;
        }
    }
}
