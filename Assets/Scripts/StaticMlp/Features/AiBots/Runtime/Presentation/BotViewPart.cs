using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class BotViewPart : MonoBehaviour, IEntityViewPart
    {
        [SerializeField] private Color bodyColor = new(0.72f, 0.24f, 0.18f);
        [SerializeField] private Color accentColor = new(0.9f, 0.78f, 0.42f);
        [SerializeField] private Vector3 visualRootLocalPosition = new(0f, 1.1f, 0f);
        [SerializeField] private Vector3 bodyScale = new(0.85f, 1.1f, 0.85f);
        [SerializeField] private Vector3 headLocalPosition = new(0f, 0.95f, 0f);
        [SerializeField] private Vector3 headScale = new(0.45f, 0.45f, 0.45f);

        private GameObject _visualRoot;

        public void OnBind(IEntityView view)
        {
            DestroyVisual();
            _visualRoot = new GameObject("Bot Visual");
            _visualRoot.transform.SetParent(transform, worldPositionStays: false);
            _visualRoot.transform.localPosition = visualRootLocalPosition;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(_visualRoot.transform, worldPositionStays: false);
            body.transform.localScale = bodyScale;
            body.GetComponent<Renderer>().sharedMaterial = RuntimeVisualMaterial.Create(bodyColor);
            Destroy(body.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(_visualRoot.transform, worldPositionStays: false);
            head.transform.localPosition = headLocalPosition;
            head.transform.localScale = headScale;
            head.GetComponent<Renderer>().sharedMaterial = RuntimeVisualMaterial.Create(accentColor);
            Destroy(head.GetComponent<Collider>());
        }

        public void OnUnbind()
        {
            DestroyVisual();
        }

        private void OnDestroy()
        {
            DestroyVisual();
        }

        private void DestroyVisual()
        {
            if (_visualRoot == null)
                return;

            Destroy(_visualRoot);
            _visualRoot = null;
        }
    }
}
