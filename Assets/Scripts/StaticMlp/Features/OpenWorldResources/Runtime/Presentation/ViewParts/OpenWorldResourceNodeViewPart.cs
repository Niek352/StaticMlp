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

        [SerializeField] private Color _woodTrunkColor = new(0.45f, 0.26f, 0.12f, 1f);
        [SerializeField] private Color _woodCanopyColor = new(0.2f, 0.62f, 0.24f, 1f);
        [SerializeField] private Color _stoneColor = new(0.48f, 0.52f, 0.55f, 1f);

        private GameObject _visualRoot;
        private Material _trunkMaterial;
        private Material _canopyMaterial;
        private Material _stoneMaterial;
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

            BuildVisual(component.KindIdValue);
            _visualRoot.transform.localScale = Vector3.one * component.Scale;
            SetVisualActive(component.RemainingAmount > 0);
        }

        private void BuildVisual(ushort kindId)
        {
            if (_visualRoot != null && _hasActiveKind && _activeKindId == kindId)
                return;

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

        private void SetVisualActive(bool active)
        {
            if (_visualRoot != null)
                _visualRoot.SetActive(active);
        }

        private void DestroyVisual()
        {
            if (_visualRoot != null)
            {
                Destroy(_visualRoot);
                _visualRoot = null;
            }

            DestroyMaterial(ref _trunkMaterial);
            DestroyMaterial(ref _canopyMaterial);
            DestroyMaterial(ref _stoneMaterial);
            _hasActiveKind = false;
        }

        private static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        private static void DestroyMaterial(ref Material material)
        {
            if (material == null)
                return;

            Destroy(material);
            material = null;
        }

        private void OnDestroy()
        {
            DestroyVisual();
        }
    }
}
