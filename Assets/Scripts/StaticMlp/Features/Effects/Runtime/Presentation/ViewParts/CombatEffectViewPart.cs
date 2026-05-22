using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.Effects
{
    public sealed class CombatEffectViewPart : MonoBehaviour, IEntityViewPart<CombatEffectVisualState>
    {
        [Header("Fallback Colors")]
        [SerializeField] private Color _poisonColor = new(0.25f, 0.9f, 0.3f, 0.35f);
        [SerializeField] private Color _fireColor = new(1f, 0.45f, 0.15f, 0.35f);
        [SerializeField] private Color _poolColor = new(1f, 0.35f, 0.1f, 0.28f);
        [SerializeField] private float _height = 0.08f;

        [Header("Particle Systems (optional — overrides primitive fallback when assigned)")]
        [SerializeField] private ParticleSystem _poisonBurstParticles;
        [SerializeField] private ParticleSystem _fireBurstParticles;
        [SerializeField] private ParticleSystem _burningPoolParticles;

        private GameObject _visual;
        private Material _material;

        public void OnBind(IEntityView view)
        {
            EnsureVisual();

            if (view.Entity.Has<CombatEffectVisualState>())
            {
                ref readonly var state = ref view.Entity.Read<CombatEffectVisualState>();
                Apply(in state);
            }
        }

        public void OnUnbind()
        {
            StopAllParticles();
            if (_visual != null)
                _visual.SetActive(false);
        }

        public void Apply(in CombatEffectVisualState component)
        {
            EnsureVisual();

            if (component.RemainingLifetime <= 0f || component.TotalLifetime <= 0f)
            {
                StopAllParticles();
                _visual.SetActive(false);
                return;
            }

            var worldPosition = component.Position + Vector3.up * 0.05f;
            var ps = ResolveParticles(component.EffectType);

            if (ps != null)
            {
                _visual.SetActive(false);
                ps.transform.position = worldPosition;
                if (!ps.isPlaying)
                    ps.Play();
                return;
            }

            // Primitive fallback
            transform.position = worldPosition;
            transform.rotation = Quaternion.identity;

            var t = Mathf.Clamp01(component.RemainingLifetime / component.TotalLifetime);
            var color = ResolveColor(component.EffectType);
            color.a *= t;
            _material.color = color;

            var diameter = Mathf.Max(0.1f, component.Radius * 2f);
            _visual.transform.localScale = new Vector3(diameter, _height, diameter);
            _visual.SetActive(true);
        }

        private void EnsureVisual()
        {
            if (_visual != null)
                return;

            _visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _visual.name = "Combat Effect Visual";
            _visual.transform.SetParent(transform, worldPositionStays: false);
            _visual.transform.localPosition = Vector3.zero;
            var collider = _visual.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            _material = RuntimeVisualMaterial.Create(_fireColor, transparent: true);
            _visual.GetComponent<Renderer>().sharedMaterial = _material;
            _visual.SetActive(false);
        }

        private ParticleSystem ResolveParticles(CombatEffectVisualType type)
        {
            switch (type)
            {
                case CombatEffectVisualType.PoisonBurst:
                    return _poisonBurstParticles;
                case CombatEffectVisualType.FireBurst:
                    return _fireBurstParticles;
                case CombatEffectVisualType.BurningPool:
                    return _burningPoolParticles;
                default:
                    return null;
            }
        }

        private Color ResolveColor(CombatEffectVisualType type)
        {
            switch (type)
            {
                case CombatEffectVisualType.PoisonBurst:
                    return _poisonColor;
                case CombatEffectVisualType.BurningPool:
                    return _poolColor;
                default:
                    return _fireColor;
            }
        }

        private void StopAllParticles()
        {
            StopParticles(_poisonBurstParticles);
            StopParticles(_fireBurstParticles);
            StopParticles(_burningPoolParticles);
        }

        private static void StopParticles(ParticleSystem ps)
        {
            if (ps != null && ps.isPlaying)
                ps.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }
    }
}
