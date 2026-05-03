using System.Collections.Generic;
using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class WoodenHutViewPart : MonoBehaviour,
        IEntityViewPart<PlacementPreviewViewState>,
        IEntityViewPart<ConstructionViewState>
    {
        [SerializeField] private WoodenHutViewMode mode;
        [SerializeField] private Color validPreviewColor = new(0.3f, 0.9f, 0.55f, 0.55f);
        [SerializeField] private Color invalidPreviewColor = new(0.95f, 0.28f, 0.24f, 0.55f);
        [SerializeField] private Color blueprintColor = new(0.32f, 0.62f, 0.9f, 0.72f);
        [SerializeField] private Color finishedWallColor = new(0.55f, 0.38f, 0.22f, 1f);
        [SerializeField] private Color finishedRoofColor = new(0.38f, 0.18f, 0.12f, 1f);
        [SerializeField] private Color progressColor = new(0.35f, 0.9f, 0.55f, 1f);
        [SerializeField] private Color missingResourcesColor = new(0.92f, 0.72f, 0.28f, 1f);

        private readonly List<Renderer> _paintedRenderers = new();
        private Transform _progressFill;
        private Renderer _progressRenderer;
        private Vector3 _progressFillFullScale;

        private void Awake()
        {
            BuildVisual();
        }

        public void OnBind(IEntityView view)
        {
            if (mode == WoodenHutViewMode.GhostPreview && view.Entity.Has<PlacementPreviewViewState>())
            {
                ref readonly var preview = ref view.Entity.Read<PlacementPreviewViewState>();
                Apply(in preview);
            }

            if (view.Entity.Has<ConstructionViewState>())
            {
                ref readonly var construction = ref view.Entity.Read<ConstructionViewState>();
                Apply(in construction);
            }
        }

        public void OnUnbind()
        {
        }

        public void Apply(in PlacementPreviewViewState component)
        {
            if (mode != WoodenHutViewMode.GhostPreview)
                return;

            PaintAll(component.IsValid ? validPreviewColor : invalidPreviewColor);
        }

        public void Apply(in ConstructionViewState component)
        {
            if (mode != WoodenHutViewMode.Blueprint)
                return;

            var progress = Mathf.Clamp01(component.Progress01);
            var width = _progressFillFullScale.x * progress;
            _progressFill.localScale = new Vector3(width, _progressFillFullScale.y, _progressFillFullScale.z);
            _progressFill.localPosition = new Vector3(-_progressFillFullScale.x * 0.5f + width * 0.5f, 1.8f, -2.68f);
            _progressRenderer.material.color = component.WoodDelivered >= component.WoodRequired
                                               && component.StoneDelivered >= component.StoneRequired
                ? progressColor
                : missingResourcesColor;
        }

        private void BuildVisual()
        {
            _paintedRenderers.Clear();

            switch (mode)
            {
                case WoodenHutViewMode.GhostPreview:
                    AddBase(validPreviewColor);
                    AddFrame(validPreviewColor);
                    break;
                case WoodenHutViewMode.Blueprint:
                    AddBase(blueprintColor);
                    AddFrame(blueprintColor);
                    AddProgressBar();
                    break;
                case WoodenHutViewMode.Finished:
                    AddBase(finishedWallColor);
                    AddFinishedHut();
                    break;
            }
        }

        private void AddBase(Color color)
        {
            AddCube("Footprint", new Vector3(0f, 0.03f, 0f), new Vector3(4f, 0.06f, 5f), color, true);
        }

        private void AddFrame(Color color)
        {
            AddCube("Post NW", new Vector3(-1.8f, 0.65f, 2.3f), new Vector3(0.12f, 1.3f, 0.12f), color, true);
            AddCube("Post NE", new Vector3(1.8f, 0.65f, 2.3f), new Vector3(0.12f, 1.3f, 0.12f), color, true);
            AddCube("Post SW", new Vector3(-1.8f, 0.65f, -2.3f), new Vector3(0.12f, 1.3f, 0.12f), color, true);
            AddCube("Post SE", new Vector3(1.8f, 0.65f, -2.3f), new Vector3(0.12f, 1.3f, 0.12f), color, true);
            AddCube("Ridge", new Vector3(0f, 1.36f, 0f), new Vector3(3.8f, 0.12f, 0.12f), color, true);
        }

        private void AddFinishedHut()
        {
            AddCube("Walls", new Vector3(0f, 0.72f, 0f), new Vector3(3.35f, 1.35f, 4.15f), finishedWallColor, true);
            AddCube("Door", new Vector3(0f, 0.42f, -2.09f), new Vector3(0.75f, 0.85f, 0.08f), new Color(0.24f, 0.13f, 0.08f, 1f), false);
            AddCube("Roof", new Vector3(0f, 1.55f, 0f), new Vector3(3.9f, 0.45f, 4.7f), finishedRoofColor, true);
        }

        private void AddProgressBar()
        {
            AddCube("Progress Back", new Vector3(0f, 1.8f, -2.7f), new Vector3(4f, 0.12f, 0.12f), new Color(0.12f, 0.16f, 0.2f, 1f), false);
            _progressFillFullScale = new Vector3(4f, 0.16f, 0.16f);
            var fill = AddCube("Progress Fill", new Vector3(-2f, 1.8f, -2.68f), _progressFillFullScale, missingResourcesColor, false);
            _progressFill = fill.transform;
            _progressFill.localScale = new Vector3(0f, _progressFillFullScale.y, _progressFillFullScale.z);
            _progressRenderer = fill.GetComponent<Renderer>();
        }

        private GameObject AddCube(string objectName, Vector3 localPosition, Vector3 localScale, Color color, bool paintWithState)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(transform, worldPositionStays: false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            Object.Destroy(cube.GetComponent<Collider>());

            var renderer = cube.GetComponent<Renderer>();
            renderer.material.color = color;
            if (paintWithState)
                _paintedRenderers.Add(renderer);

            return cube;
        }

        private void PaintAll(Color color)
        {
            for (var i = 0; i < _paintedRenderers.Count; i++)
                _paintedRenderers[i].material.color = color;
        }
    }
}
