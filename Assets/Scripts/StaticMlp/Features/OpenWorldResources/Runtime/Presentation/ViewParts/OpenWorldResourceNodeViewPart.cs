using System;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourceNodeViewPart : MonoBehaviour, IEntityViewPart<OpenWorldResourceNodeViewState>
    {
        private const ushort WOOD_KIND = OpenWorldGenerationConfig.TREE_RESOURCE_KIND;
        private const ushort STONE_KIND = OpenWorldGenerationConfig.ORE_RESOURCE_KIND;
        private const ushort SPORE_POD_KIND = OpenWorldGenerationConfig.SPORE_POD_RESOURCE_KIND;
        private const ushort CHEST_KIND = OpenWorldGenerationConfig.CHEST_RESOURCE_KIND;
        private const int AMOUNT_PIP_COUNT = 5;

        [SerializeField] private Color _woodTrunkColor = new(0.45f, 0.26f, 0.12f, 1f);
        [SerializeField] private Color _woodCanopyColor = new(0.2f, 0.62f, 0.24f, 1f);
        [SerializeField] private Color _stoneColor = new(0.48f, 0.52f, 0.55f, 1f);
        [SerializeField] private Color _sporeStemColor = new(0.53f, 0.47f, 0.74f, 1f);
        [SerializeField] private Color _sporeCapColor = new(0.77f, 0.33f, 0.68f, 1f);
        [SerializeField] private Color _chestBodyColor = new(0.52f, 0.31f, 0.14f, 1f);
        [SerializeField] private Color _chestBandColor = new(0.86f, 0.68f, 0.28f, 1f);
        [SerializeField] private Color _amountPipColor = new(0.95f, 0.78f, 0.28f, 1f);
        [SerializeField] private Color _hitFlashColor = new(1f, 0.95f, 0.55f, 1f);
        [SerializeField] private Color _depletionPulseColor = new(1f, 0.35f, 0.18f, 1f);

        private GameObject _visualRoot;
        private GameObject _amountIndicatorRoot;
        private GameObject[] _amountPips;
        private Material _trunkMaterial;
        private Material _canopyMaterial;
        private Material _stoneMaterial;
        private Material _sporeStemMaterial;
        private Material _sporeCapMaterial;
        private Material _chestBodyMaterial;
        private Material _chestBandMaterial;
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
            if (kindId != WOOD_KIND
                && kindId != STONE_KIND
                && kindId != SPORE_POD_KIND
                && kindId != CHEST_KIND)
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
                case SPORE_POD_KIND:
                    BuildSporePodVisual(_visualRoot.transform);
                    break;
                case CHEST_KIND:
                    BuildChestVisual(_visualRoot.transform);
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

        private void BuildSporePodVisual(Transform root)
        {
            var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Spore Stem";
            stem.transform.SetParent(root, worldPositionStays: false);
            stem.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            stem.transform.localScale = new Vector3(0.22f, 0.45f, 0.22f);
            RemoveCollider(stem);
            _sporeStemMaterial = RuntimeVisualMaterial.Create(_sporeStemColor);
            stem.GetComponent<Renderer>().sharedMaterial = _sporeStemMaterial;

            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Spore Cap";
            cap.transform.SetParent(root, worldPositionStays: false);
            cap.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            cap.transform.localScale = new Vector3(0.95f, 0.5f, 0.95f);
            RemoveCollider(cap);
            _sporeCapMaterial = RuntimeVisualMaterial.Create(_sporeCapColor);
            cap.GetComponent<Renderer>().sharedMaterial = _sporeCapMaterial;
        }

        private void BuildChestVisual(Transform root)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Resource Chest Body";
            body.transform.SetParent(root, worldPositionStays: false);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.transform.localScale = new Vector3(1.1f, 0.55f, 0.75f);
            RemoveCollider(body);
            _chestBodyMaterial = RuntimeVisualMaterial.Create(_chestBodyColor);
            body.GetComponent<Renderer>().sharedMaterial = _chestBodyMaterial;

            var lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "Resource Chest Lid";
            lid.transform.SetParent(root, worldPositionStays: false);
            lid.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            lid.transform.localScale = new Vector3(1.18f, 0.22f, 0.82f);
            RemoveCollider(lid);
            lid.GetComponent<Renderer>().sharedMaterial = _chestBodyMaterial;

            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "Resource Chest Band";
            band.transform.SetParent(root, worldPositionStays: false);
            band.transform.localPosition = new Vector3(0f, 0.58f, -0.42f);
            band.transform.localScale = new Vector3(0.18f, 0.72f, 0.08f);
            RemoveCollider(band);
            _chestBandMaterial = RuntimeVisualMaterial.Create(_chestBandColor);
            band.GetComponent<Renderer>().sharedMaterial = _chestBandMaterial;
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
            DestroyMaterial(ref _sporeStemMaterial);
            DestroyMaterial(ref _sporeCapMaterial);
            DestroyMaterial(ref _chestBodyMaterial);
            DestroyMaterial(ref _chestBandMaterial);
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
                case SPORE_POD_KIND:
                    ApplyMaterialFeedback(_sporeStemMaterial, _sporeStemColor, feedbackColor, intensity);
                    ApplyMaterialFeedback(_sporeCapMaterial, _sporeCapColor, feedbackColor, intensity);
                    break;
                case CHEST_KIND:
                    ApplyMaterialFeedback(_chestBodyMaterial, _chestBodyColor, feedbackColor, intensity);
                    ApplyMaterialFeedback(_chestBandMaterial, _chestBandColor, feedbackColor, intensity);
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
