using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcePickupViewPart : MonoBehaviour, IEntityViewPart<ResourcePickupViewState>
    {
        private static readonly Color COLOR_DEFAULT = new(0.95f, 0.85f, 0.2f, 1f);
        private static readonly Color COLOR_MAGNETIZED = new(1f, 0.55f, 0.05f, 1f);

        private GameObject _orb;
        private Material _orbMaterial;

        public void OnBind(IEntityView view)
        {
            BuildOrb();

            if (view.Entity.Has<ResourcePickupViewState>())
            {
                ref readonly var state = ref view.Entity.Read<ResourcePickupViewState>();
                Apply(in state);
            }
        }

        public void OnUnbind()
        {
            DestroyOrb();
        }

        public void Apply(in ResourcePickupViewState component)
        {
            if (_orbMaterial == null)
                return;

            _orb.SetActive(!component.IsConsumed);
            if (component.IsConsumed)
                return;

            _orbMaterial.color = component.IsMagnetized ? COLOR_MAGNETIZED : COLOR_DEFAULT;
            var scale = component.IsMagnetized ? 0.22f : 0.3f;
            _orb.transform.localScale = Vector3.one * scale;
        }

        private void BuildOrb()
        {
            _orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _orb.name = "Resource Pickup Orb";
            _orb.transform.SetParent(transform, worldPositionStays: false);
            _orb.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            _orb.transform.localScale = Vector3.one * 0.3f;

            var col = _orb.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            _orbMaterial = new Material(Shader.Find("Standard"))
            {
                color = COLOR_DEFAULT
            };
            _orb.GetComponent<Renderer>().sharedMaterial = _orbMaterial;
        }

        private void DestroyOrb()
        {
            if (_orb != null)
            {
                Destroy(_orb);
                _orb = null;
            }

            if (_orbMaterial != null)
            {
                Destroy(_orbMaterial);
                _orbMaterial = null;
            }
        }

        private void OnDestroy()
        {
            DestroyOrb();
        }
    }
}
