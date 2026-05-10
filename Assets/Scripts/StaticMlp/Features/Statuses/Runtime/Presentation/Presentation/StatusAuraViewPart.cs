using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.Statuses
{
    public sealed class StatusAuraViewPart : MonoBehaviour, IEntityViewPart<StatusAuraViewState>
    {
        [SerializeField] private Color _poisonAuraColor = new(0.25f, 0.8f, 0.25f, 0.45f);
        [SerializeField] private Color _burningAuraColor = new(1f, 0.45f, 0.15f, 0.55f);
        [SerializeField] private Color _oiledAuraColor = new(0.15f, 0.15f, 0.15f, 0.5f);
        [SerializeField] private Vector3 _statusAuraLocalPosition = new(0f, 0.35f, 0f);
        [SerializeField] private Vector3 _statusAuraBaseScale = new(1.25f, 1.25f, 1.25f);

        private GameObject _statusAuraObject;
        private Material _statusAuraMaterial;

        public void OnBind(IEntityView view)
        {
            EnsureStatusAura();

            if (view.Entity.Has<StatusAuraViewState>())
            {
                ref readonly var state = ref view.Entity.Read<StatusAuraViewState>();
                Apply(in state);
            }
            else
            {
                HideStatusAura();
            }
        }

        public void OnUnbind()
        {
            HideStatusAura();
        }

        public void Apply(in StatusAuraViewState component)
        {
            EnsureStatusAura();
            if (component.Flags == StatusVisualFlags.None)
            {
                HideStatusAura();
                return;
            }

            var color = ResolveStatusColor(component.Flags);
            if (component.IsDead)
                color.a *= 0.2f;

            _statusAuraMaterial.color = color;
            _statusAuraObject.SetActive(true);
            _statusAuraObject.transform.localPosition = _statusAuraLocalPosition;
            _statusAuraObject.transform.localScale = Vector3.Lerp(
                _statusAuraBaseScale * 0.9f,
                _statusAuraBaseScale * 1.15f,
                1f - Mathf.Clamp01(component.HealthNormalized));
        }

        private void EnsureStatusAura()
        {
            if (_statusAuraObject != null)
                return;

            _statusAuraObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _statusAuraObject.name = "Combat Status Aura";
            _statusAuraObject.transform.SetParent(transform, worldPositionStays: false);
            _statusAuraObject.transform.localPosition = _statusAuraLocalPosition;
            _statusAuraObject.transform.localScale = _statusAuraBaseScale;
            var collider = _statusAuraObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            _statusAuraMaterial = RuntimeVisualMaterial.Create(_poisonAuraColor, transparent: true);
            _statusAuraObject.GetComponent<Renderer>().sharedMaterial = _statusAuraMaterial;
            HideStatusAura();
        }

        private void HideStatusAura()
        {
            if (_statusAuraObject == null)
                return;

            _statusAuraObject.SetActive(false);
        }

        private Color ResolveStatusColor(StatusVisualFlags flags)
        {
            if ((flags & StatusVisualFlags.Burning) != 0)
                return _burningAuraColor;

            if ((flags & StatusVisualFlags.Poison) != 0)
                return _poisonAuraColor;

            return _oiledAuraColor;
        }

        private void OnDestroy()
        {
            if (_statusAuraMaterial != null)
                Destroy(_statusAuraMaterial);
        }
    }
}
