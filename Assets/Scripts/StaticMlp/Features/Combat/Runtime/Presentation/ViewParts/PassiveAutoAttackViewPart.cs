using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class PassiveAutoAttackViewPart : MonoBehaviour,
        IEntityViewPart<PassiveAutoAttackViewState>,
        IEntityViewPart<PassiveAutoAttackTargetViewState>
    {
        [SerializeField] private Color _tracerColor = new(1f, 0.35f, 0.15f, 0.95f);
        [SerializeField] private Color _highlightColor = new(1f, 0.75f, 0.2f, 0.45f);
        [SerializeField] private float _tracerWidth = 0.08f;
        [SerializeField] private Vector3 _highlightLocalPosition = new(0f, 0.1f, 0f);
        [SerializeField] private Vector3 _highlightBaseScale = new(1.5f, 0.04f, 1.5f);

        private GameObject _tracerObject;
        private LineRenderer _tracer;
        private Material _tracerMaterial;
        private GameObject _highlightObject;
        private Material _highlightMaterial;

        public void OnBind(IEntityView view)
        {
            EnsureVisuals();

            if (view.Entity.Has<PassiveAutoAttackViewState>())
            {
                ref readonly var tracerState = ref view.Entity.Read<PassiveAutoAttackViewState>();
                Apply(in tracerState);
            }
            else
            {
                HideTracer();
            }

            if (view.Entity.Has<PassiveAutoAttackTargetViewState>())
            {
                ref readonly var targetState = ref view.Entity.Read<PassiveAutoAttackTargetViewState>();
                Apply(in targetState);
            }
            else
            {
                HideHighlight();
            }
        }

        public void OnUnbind()
        {
            HideTracer();
            HideHighlight();
        }

        public void Apply(in PassiveAutoAttackViewState component)
        {
            EnsureVisuals();
            if (!component.HasTracer || component.RemainingLifetime <= 0f)
            {
                HideTracer();
                return;
            }

            _tracerObject.SetActive(true);
            _tracer.enabled = true;
            _tracer.startWidth = _tracerWidth;
            _tracer.endWidth = _tracerWidth;
            _tracer.positionCount = 2;
            _tracer.useWorldSpace = true;
            _tracer.SetPosition(0, component.TracerStart);
            _tracer.SetPosition(1, component.TracerEnd);
        }

        public void Apply(in PassiveAutoAttackTargetViewState component)
        {
            EnsureVisuals();
            if (!component.IsHighlighted && component.HighlightIntensity <= 0f)
            {
                HideHighlight();
                return;
            }

            var color = _highlightColor;
            color.a *= Mathf.Clamp01(component.HighlightIntensity);
            _highlightMaterial.color = color;
            _highlightObject.SetActive(true);
            _highlightObject.transform.localPosition = _highlightLocalPosition;
            _highlightObject.transform.localScale = _highlightBaseScale;
        }

        private void EnsureVisuals()
        {
            if (_tracer == null)
                CreateTracer();

            if (_highlightObject == null)
                CreateHighlight();
        }

        private void CreateTracer()
        {
            _tracerObject = new GameObject("Combat Tracer");
            _tracerObject.transform.SetParent(transform, worldPositionStays: false);
            _tracer = _tracerObject.AddComponent<LineRenderer>();
            _tracerMaterial = RuntimeVisualMaterial.Create(_tracerColor, transparent: true);
            _tracer.sharedMaterial = _tracerMaterial;
            _tracer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _tracer.receiveShadows = false;
            _tracer.textureMode = LineTextureMode.Stretch;
            _tracer.alignment = LineAlignment.View;
            _tracer.numCapVertices = 2;
            HideTracer();
        }

        private void CreateHighlight()
        {
            _highlightObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _highlightObject.name = "Combat Target Highlight";
            _highlightObject.transform.SetParent(transform, worldPositionStays: false);
            _highlightObject.transform.localPosition = _highlightLocalPosition;
            _highlightObject.transform.localScale = _highlightBaseScale;
            var collider = _highlightObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            _highlightMaterial = RuntimeVisualMaterial.Create(_highlightColor, transparent: true);
            _highlightObject.GetComponent<Renderer>().sharedMaterial = _highlightMaterial;
            HideHighlight();
        }

        private void HideTracer()
        {
            if (_tracerObject == null)
                return;

            _tracer.enabled = false;
            _tracer.positionCount = 0;
            _tracerObject.SetActive(false);
        }

        private void HideHighlight()
        {
            if (_highlightObject == null)
                return;

            _highlightObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_tracerMaterial != null)
                Destroy(_tracerMaterial);

            if (_highlightMaterial != null)
                Destroy(_highlightMaterial);
        }
    }
}
