using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.Effects
{
    public sealed class CombatProjectileViewPart : MonoBehaviour, IEntityViewPart<CombatProjectileVisualState>
    {
        [SerializeField] private Color _poisonColor = new(0.3f, 0.9f, 0.35f, 1f);
        [SerializeField] private Color _fireColor = new(1f, 0.45f, 0.1f, 1f);
        [SerializeField] private Color _defaultColor = new(0.95f, 0.85f, 0.35f, 1f);
        [SerializeField] private float _poisonScale = 0.22f;
        [SerializeField] private float _fireScale = 0.32f;
        [SerializeField] private float _defaultScale = 0.16f;

        private GameObject _visual;
        private Material _material;

        public void OnBind(IEntityView view)
        {
            EnsureVisual();

            if (view.Entity.Has<CombatProjectileVisualState>())
            {
                ref readonly var state = ref view.Entity.Read<CombatProjectileVisualState>();
                Apply(in state);
            }
        }

        public void OnUnbind()
        {
            if (_visual != null)
                _visual.SetActive(false);
        }

        public void Apply(in CombatProjectileVisualState component)
        {
            EnsureVisual();
            if (component.RemainingLifetime <= 0f || component.TotalLifetime <= 0f)
            {
                _visual.SetActive(false);
                return;
            }

            var progress = 1f - Mathf.Clamp01(component.RemainingLifetime / component.TotalLifetime);
            var position = Vector3.Lerp(component.Start, component.End, progress);
            var direction = component.End - component.Start;
            if (direction.sqrMagnitude <= 0.0001f)
                direction = Vector3.forward;

            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            var color = ResolveColor(component.AbilityId);
            _material.color = color;
            var scale = ResolveScale(component.AbilityId);
            _visual.transform.localScale = new Vector3(scale, scale, scale * 1.8f);
            _visual.SetActive(true);
        }

        private void EnsureVisual()
        {
            if (_visual != null)
                return;

            _visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _visual.name = "Projectile Visual";
            _visual.transform.SetParent(transform, worldPositionStays: false);
            _visual.transform.localPosition = Vector3.zero;
            var collider = _visual.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            _material = RuntimeVisualMaterial.Create(_defaultColor);
            _visual.GetComponent<Renderer>().sharedMaterial = _material;
            _visual.SetActive(false);
        }

        private Color ResolveColor(CombatAbilityId abilityId)
        {
            switch (abilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    return _poisonColor;
                case CombatAbilityId.FireFlask:
                    return _fireColor;
                default:
                    return _defaultColor;
            }
        }

        private float ResolveScale(CombatAbilityId abilityId)
        {
            switch (abilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    return _poisonScale;
                case CombatAbilityId.FireFlask:
                    return _fireScale;
                default:
                    return _defaultScale;
            }
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }
    }
}
