using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using UnityEngine;
using UnityEngine.Serialization;

namespace StaticMlp.Features.AiBots
{
    public sealed class BotViewPart : MonoBehaviour, IEntityViewPart
    {
        [FormerlySerializedAs("bodyColor")] [SerializeField] private Color _bodyColor = new(0.72f, 0.24f, 0.18f);
        [FormerlySerializedAs("accentColor")] [SerializeField] private Color _accentColor = new(0.9f, 0.78f, 0.42f);
        [FormerlySerializedAs("visualRootLocalPosition")] [SerializeField] private Vector3 _visualRootLocalPosition = new(0f, 1.1f, 0f);
        [FormerlySerializedAs("bodyScale")] [SerializeField] private Vector3 _bodyScale = new(0.85f, 1.1f, 0.85f);
        [FormerlySerializedAs("headLocalPosition")] [SerializeField] private Vector3 _headLocalPosition = new(0f, 0.95f, 0f);
        [FormerlySerializedAs("headScale")] [SerializeField] private Vector3 _headScale = new(0.45f, 0.45f, 0.45f);

        private GameObject _visualRoot;

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
            body.GetComponent<Renderer>().sharedMaterial = RuntimeVisualMaterial.Create(_bodyColor);
            Destroy(body.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(_visualRoot.transform, worldPositionStays: false);
            head.transform.localPosition = _headLocalPosition;
            head.transform.localScale = _headScale;
            head.GetComponent<Renderer>().sharedMaterial = RuntimeVisualMaterial.Create(_accentColor);
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
