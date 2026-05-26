using System;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourceNodeViewPart : MonoBehaviour, IEntityViewPart<OpenWorldResourceNodeViewState>
    {
        private const ushort WOOD_KIND = 1;
        private const ushort STONE_KIND = 2;
        private const int AMOUNT_PIP_COUNT = 5;

        [SerializeField] private Color _woodTrunkColor = new(0.45f, 0.26f, 0.12f, 1f);
        [SerializeField] private Color _woodCanopyColor = new(0.2f, 0.62f, 0.24f, 1f);
        [SerializeField] private Color _stoneColor = new(0.48f, 0.52f, 0.55f, 1f);
        [SerializeField] private Color _amountPipColor = new(0.95f, 0.78f, 0.28f, 1f);
        [SerializeField] private Color _hitFlashColor = new(1f, 0.95f, 0.55f, 1f);
        [SerializeField] private Color _depletionPulseColor = new(1f, 0.35f, 0.18f, 1f);

        private GameObject _visualRoot;
        private GameObject _amountIndicatorRoot;
        private GameObject[] _amountPips;
        private Material _trunkMaterial;
        private Material _canopyMaterial;
        private Material _stoneMaterial;
        private Material _amountPipMaterial;
        private ushort _activeKindId;
        private bool _hasActiveKind;

        public void OnBind(IEntityView view)
        {
            if (view.Entity.Has<OpenWorldResourceNodeViewState>())
            {
                ref readonly var state = ref view.Entity.Read<OpenWorldResourceNodeViewState>();
                Apply(in state);
            }
        }

        public void OnUnbind()
        {
            SetVisualActive(false);
        }

        public void Apply(in OpenWorldResourceNodeViewState component)
        {
            if (component.Scale <= 0f)
                throw new InvalidOperationException($"{nameof(OpenWorldResourceNodeViewState)} scale must be positive.");
            if (component.MaxAmount <= 0)
                throw new InvalidOperationException($"{nameof(OpenWorldResourceNodeViewState)} max amount must be positive.");

            BuildVisual(component.KindIdValue);
            var pulseScale = 1f + component.DepletionPulseIntensity * 0.25f;
            _visualRoot.transform.localScale = Vector3.one * component.Scale * pulseScale;
            ApplyAmountIndicator(component.RemainingAmount, component.MaxAmount);
            ApplyFeedback(component.HitFlashIntensity, component.DepletionPulseIntensity);
            SetVisualActive((!IsInactive(component.Flags) && component.RemainingAmount > 0) || component.DepletionPulseIntensity > 0f);
        }

        private void BuildVisual(ushort kindId)
        {
            if (_visualRoot != null && _hasActiveKind && _activeKindId == kindId)
                return;
            if (kindId != WOOD_KIND && kindId != STONE_KIND)
                throw new InvalidOperationException($"Unknown open world resource node kind: {kindId}.");

            DestroyVisual();
            _activeKindId = kindId;
            _hasActiveKind = true;
            _visualRoot = new GameObject("Resource Node Visual");
            _visualRoot.transform.SetParent(transform, worldPositionStays: false);
            _visualRoot.transform.localPosition = Vector3.zero;
            _visualRoot.transform.localRotation = Quaternion.identity;

            switch (kindId)
            {
                case WOOD_KIND:
                    BuildWoodVisual(_visualRoot.transform);
                    break;
                case STONE_KIND:
                    BuildStoneVisual(_visualRoot.transform);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown open world resource node kind: {kindId}.");
            }

            BuildAmountIndicator(_visualRoot.transform);
        }

        private void BuildWoodVisual(Transform root)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Wood Trunk";
            trunk.transform.SetParent(root, worldPositionStays: false);
            trunk.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            trunk.transform.localScale = new Vector3(0.28f, 0.55f, 0.28f);
            RemoveCollider(trunk);
            _trunkMaterial = RuntimeVisualMaterial.Create(_woodTrunkColor);
            trunk.GetComponent<Renderer>().sharedMaterial = _trunkMaterial;

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Wood Canopy";
            canopy.transform.SetParent(root, worldPositionStays: false);
            canopy.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            canopy.transform.localScale = new Vector3(1.1f, 0.9f, 1.1f);
            RemoveCollider(canopy);
            _canopyMaterial = RuntimeVisualMaterial.Create(_woodCanopyColor);
            canopy.GetComponent<Renderer>().sharedMaterial = _canopyMaterial;
        }

        private void BuildStoneVisual(Transform root)
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "Stone Boulder";
            stone.transform.SetParent(root, worldPositionStays: false);
            stone.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            stone.transform.localRotation = Quaternion.Euler(0f, 24f, 0f);
            stone.transform.localScale = new Vector3(1.05f, 0.7f, 0.85f);
            RemoveCollider(stone);
            _stoneMaterial = RuntimeVisualMaterial.Create(_stoneColor);
            stone.GetComponent<Renderer>().sharedMaterial = _stoneMaterial;
        }

        private void BuildAmountIndicator(Transform root)
        {
            _amountIndicatorRoot = new GameObject("Resource Amount Indicator");
            _amountIndicatorRoot.transform.SetParent(root, worldPositionStays: false);
            _amountIndicatorRoot.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            _amountIndicatorRoot.transform.localRotation = Quaternion.identity;

            _amountPipMaterial = RuntimeVisualMaterial.Create(_amountPipColor);
            _amountPips = new GameObject[AMOUNT_PIP_COUNT];
            var firstPipX = (AMOUNT_PIP_COUNT - 1) * -0.13f;
            for (var i = 0; i < AMOUNT_PIP_COUNT; i++)
            {
                var pip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pip.name = $"Resource Amount Pip {i + 1}";
                pip.transform.SetParent(_amountIndicatorRoot.transform, worldPositionStays: false);
                pip.transform.localPosition = new Vector3(firstPipX + i * 0.26f, 0f, 0f);
                pip.transform.localScale = new Vector3(0.18f, 0.18f, 0.08f);
                RemoveCollider(pip);
                pip.GetComponent<Renderer>().sharedMaterial = _amountPipMaterial;
                _amountPips[i] = pip;
            }
        }

        private void ApplyAmountIndicator(int remainingAmount, int maxAmount)
        {
            if (_amountPips == null)
                throw new InvalidOperationException($"{nameof(OpenWorldResourceNodeViewPart)} amount indicator is not built.");

            var normalized = Mathf.Clamp01((float)remainingAmount / maxAmount);
            var activePips = remainingAmount <= 0 ? 0 : Mathf.Max(1, Mathf.CeilToInt(normalized * AMOUNT_PIP_COUNT));
            for (var i = 0; i < _amountPips.Length; i++)
                _amountPips[i].SetActive(i < activePips);
        }

        private void SetVisualActive(bool active)
        {
            if (_visualRoot != null)
                _visualRoot.SetActive(active);
        }

        private void DestroyVisual()
        {
            if (_visualRoot != null)
            {
                DestroyRuntimeObject(_visualRoot);
                _visualRoot = null;
            }

            _amountIndicatorRoot = null;
            _amountPips = null;
            DestroyMaterial(ref _trunkMaterial);
            DestroyMaterial(ref _canopyMaterial);
            DestroyMaterial(ref _stoneMaterial);
            DestroyMaterial(ref _amountPipMaterial);
            _hasActiveKind = false;
        }

        private void ApplyFeedback(float hitFlashIntensity, float depletionPulseIntensity)
        {
            var feedbackColor = Color.Lerp(_hitFlashColor, _depletionPulseColor, depletionPulseIntensity);
            var intensity = Mathf.Clamp01(Mathf.Max(hitFlashIntensity, depletionPulseIntensity));

            switch (_activeKindId)
            {
                case WOOD_KIND:
                    ApplyMaterialFeedback(_trunkMaterial, _woodTrunkColor, feedbackColor, intensity);
                    ApplyMaterialFeedback(_canopyMaterial, _woodCanopyColor, feedbackColor, intensity);
                    break;
                case STONE_KIND:
                    ApplyMaterialFeedback(_stoneMaterial, _stoneColor, feedbackColor, intensity);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown open world resource node kind: {_activeKindId}.");
            }
        }

        private static void ApplyMaterialFeedback(Material material, Color baseColor, Color feedbackColor, float intensity)
        {
            if (material == null)
                throw new InvalidOperationException($"{nameof(OpenWorldResourceNodeViewPart)} feedback material is not built.");

            material.color = Color.Lerp(baseColor, feedbackColor, intensity);
        }

        private static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
                DestroyRuntimeObject(collider);
        }

        private static void DestroyMaterial(ref Material material)
        {
            if (material == null)
                return;

            DestroyRuntimeObject(material);
            material = null;
        }

        private static bool IsInactive(OpenWorldResourceOverlayFlags flags)
        {
            const OpenWorldResourceOverlayFlags inactive =
                OpenWorldResourceOverlayFlags.Depleted
                | OpenWorldResourceOverlayFlags.Hidden
                | OpenWorldResourceOverlayFlags.Replaced;
            return (flags & inactive) != 0;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

        private void OnDestroy()
        {
            DestroyVisual();
        }
    }
}
