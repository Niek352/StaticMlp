using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatHealthViewPart : MonoBehaviour,
        IEntityViewPart<CombatHealthViewState>,
        IEntityViewPart<DamageFeedbackViewState>
    {
        [SerializeField] private Color _damageFlashColor = new(1f, 0.15f, 0.15f, 0.75f);
        [SerializeField] private Vector3 _damageFlashLocalPosition = new(0f, 1f, 0f);
        [SerializeField] private Vector3 _damageFlashBaseScale = new(1.1f, 1.1f, 1.1f);

        private GameObject _damageFlashObject;
        private Material _damageFlashMaterial;
        private float _healthNormalized = 1f;
        private bool _isDead;

        public void OnBind(IEntityView view)
        {
            EnsureDamageFlash();

            if (view.Entity.Has<DamageFeedbackViewState>())
            {
                ref readonly var damageState = ref view.Entity.Read<DamageFeedbackViewState>();
                Apply(in damageState);
            }
            else
            {
                HideDamageFlash();
            }

            if (view.Entity.Has<CombatHealthViewState>())
            {
                ref readonly var healthState = ref view.Entity.Read<CombatHealthViewState>();
                Apply(in healthState);
            }
        }

        public void OnUnbind()
        {
            HideDamageFlash();
        }

        public void Apply(in CombatHealthViewState component)
        {
            _healthNormalized = Mathf.Clamp01(component.HealthNormalized);
            _isDead = component.IsDead;
        }

        public void Apply(in DamageFeedbackViewState component)
        {
            EnsureDamageFlash();
            if (!component.IsActive || component.Intensity <= 0f)
            {
                HideDamageFlash();
                return;
            }

            var color = _damageFlashColor;
            color.a *= Mathf.Clamp01(component.Intensity);
            if (_isDead)
                color.a *= 0.35f;

            _damageFlashMaterial.color = color;
            _damageFlashObject.SetActive(true);
            _damageFlashObject.transform.localPosition = _damageFlashLocalPosition;
            _damageFlashObject.transform.localScale = Vector3.Lerp(
                _damageFlashBaseScale * (1.2f + (1f - _healthNormalized) * 0.35f),
                _damageFlashBaseScale,
                Mathf.Clamp01(component.Intensity));
        }

        private void EnsureDamageFlash()
        {
            if (_damageFlashObject == null)
                CreateDamageFlash();
        }

        private void CreateDamageFlash()
        {
            _damageFlashObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _damageFlashObject.name = "Combat Damage Flash";
            _damageFlashObject.transform.SetParent(transform, worldPositionStays: false);
            _damageFlashObject.transform.localPosition = _damageFlashLocalPosition;
            _damageFlashObject.transform.localScale = _damageFlashBaseScale;
            var collider = _damageFlashObject.GetComponent<Collider>();
            if (collider != null)
                DestroyUnityObject(collider);

            _damageFlashMaterial = RuntimeVisualMaterial.Create(_damageFlashColor, transparent: true);
            _damageFlashObject.GetComponent<Renderer>().sharedMaterial = _damageFlashMaterial;
            HideDamageFlash();
        }

        private void HideDamageFlash()
        {
            if (_damageFlashObject == null)
                return;

            _damageFlashObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_damageFlashMaterial != null)
                DestroyUnityObject(_damageFlashMaterial);
        }

        private static void DestroyUnityObject(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }
    }
}
