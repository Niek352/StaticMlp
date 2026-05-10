using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class BotViewPart : MonoBehaviour, IEntityViewPart, IEntityViewPart<CombatHealthViewState>
    {
        [SerializeField] private Color _bodyColor = new(0.72f, 0.24f, 0.18f);
        [SerializeField] private Color _accentColor = new(0.9f, 0.78f, 0.42f);
        [SerializeField] private Vector3 _visualRootLocalPosition = new(0f, 1.1f, 0f);
        [SerializeField] private Vector3 _bodyScale = new(0.85f, 1.1f, 0.85f);
        [SerializeField] private Vector3 _headLocalPosition = new(0f, 0.95f, 0f);
        [SerializeField] private Vector3 _headScale = new(0.45f, 0.45f, 0.45f);
        [SerializeField] private Vector3 _healthBarLocalPosition = new(0f, 2.05f, 0f);
        [SerializeField] private Vector3 _healthBarBackgroundScale = new(1.2f, 0.12f, 0.12f);
        [SerializeField] private float _healthBarFillDepthOffset = -0.02f;
        [SerializeField] private Color _healthBarBackgroundColor = new(0.14f, 0.05f, 0.05f);
        [SerializeField] private Color _healthBarLowColor = new(0.88f, 0.2f, 0.18f);
        [SerializeField] private Color _healthBarHighColor = new(0.2f, 0.82f, 0.32f);

        private GameObject _visualRoot;
        private GameObject _healthBarRoot;
        private GameObject _healthBarFillObject;
        private Material _bodyMaterial;
        private Material _headMaterial;
        private Material _healthBarBackgroundMaterial;
        private Material _healthBarFillMaterial;

        public void OnBind(IEntityView view)
        {
            DestroyVisual();
            _visualRoot = new GameObject("Bot Visual");
            _visualRoot.transform.SetParent(transform, worldPositionStays: false);
            _visualRoot.transform.localPosition = _visualRootLocalPosition;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(_visualRoot.transform, worldPositionStays: false);
            body.transform.localScale = _bodyScale;
            _bodyMaterial = RuntimeVisualMaterial.Create(_bodyColor);
            body.GetComponent<Renderer>().sharedMaterial = _bodyMaterial;
            Destroy(body.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(_visualRoot.transform, worldPositionStays: false);
            head.transform.localPosition = _headLocalPosition;
            head.transform.localScale = _headScale;
            _headMaterial = RuntimeVisualMaterial.Create(_accentColor);
            head.GetComponent<Renderer>().sharedMaterial = _headMaterial;
            Destroy(head.GetComponent<Collider>());

            EnsureHealthBar();
            ApplyHealthBar(1f, isDead: false);
        }

        public void OnUnbind()
        {
            DestroyVisual();
        }

        public void Apply(in CombatHealthViewState component)
        {
            EnsureHealthBar();
            ApplyHealthBar(component.HealthNormalized, component.IsDead);
        }

        private void OnDestroy()
        {
            DestroyVisual();
        }

        private void DestroyVisual()
        {
            if (_visualRoot == null)
            {
                DestroyHealthBar();
                DestroyRuntimeMaterials();
                return;
            }

            Destroy(_visualRoot);
            _visualRoot = null;
            DestroyHealthBar();
            DestroyRuntimeMaterials();
        }

        private void EnsureHealthBar()
        {
            if (_healthBarRoot != null)
                return;

            _healthBarRoot = new GameObject("Health Bar Root");
            _healthBarRoot.transform.SetParent(transform, worldPositionStays: false);
            _healthBarRoot.transform.localPosition = _healthBarLocalPosition;

            var background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Health Bar Background";
            background.transform.SetParent(_healthBarRoot.transform, worldPositionStays: false);
            background.transform.localScale = _healthBarBackgroundScale;
            var backgroundCollider = background.GetComponent<Collider>();
            if (backgroundCollider != null)
                Destroy(backgroundCollider);

            _healthBarBackgroundMaterial = RuntimeVisualMaterial.Create(_healthBarBackgroundColor);
            background.GetComponent<Renderer>().sharedMaterial = _healthBarBackgroundMaterial;

            _healthBarFillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _healthBarFillObject.name = "Health Bar Fill";
            _healthBarFillObject.transform.SetParent(_healthBarRoot.transform, worldPositionStays: false);
            var fillCollider = _healthBarFillObject.GetComponent<Collider>();
            if (fillCollider != null)
                Destroy(fillCollider);

            _healthBarFillMaterial = RuntimeVisualMaterial.Create(_healthBarHighColor);
            _healthBarFillObject.GetComponent<Renderer>().sharedMaterial = _healthBarFillMaterial;
        }

        private void ApplyHealthBar(float healthNormalized, bool isDead)
        {
            var normalized = Mathf.Clamp01(healthNormalized);
            _healthBarRoot.transform.localPosition = _healthBarLocalPosition;

            if (normalized <= 0f)
            {
                _healthBarFillObject.SetActive(false);
                return;
            }

            var fillWidth = _healthBarBackgroundScale.x * normalized;
            _healthBarFillObject.SetActive(true);
            _healthBarFillObject.transform.localScale = new Vector3(
                fillWidth,
                _healthBarBackgroundScale.y * 0.72f,
                _healthBarBackgroundScale.z * 0.72f);
            _healthBarFillObject.transform.localPosition = new Vector3(
                (fillWidth - _healthBarBackgroundScale.x) * 0.5f,
                0f,
                _healthBarFillDepthOffset);

            var fillColor = Color.Lerp(_healthBarLowColor, _healthBarHighColor, normalized);
            if (isDead)
                fillColor *= 0.55f;

            _healthBarFillMaterial.color = fillColor;
        }

        private void DestroyHealthBar()
        {
            if (_healthBarRoot == null)
                return;

            Destroy(_healthBarRoot);
            _healthBarRoot = null;
            _healthBarFillObject = null;
        }

        private void DestroyRuntimeMaterials()
        {
            DestroyRuntimeMaterial(ref _bodyMaterial);
            DestroyRuntimeMaterial(ref _headMaterial);
            DestroyRuntimeMaterial(ref _healthBarBackgroundMaterial);
            DestroyRuntimeMaterial(ref _healthBarFillMaterial);
        }

        private static void DestroyRuntimeMaterial(ref Material material)
        {
            if (material == null)
                return;

            Destroy(material);
            material = null;
        }
    }
}
