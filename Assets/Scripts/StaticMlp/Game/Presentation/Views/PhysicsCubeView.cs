using UnityEngine;

namespace StaticMlp.Game.Presentation.Views
{
    public sealed class PhysicsCubeView : MonoBehaviour
    {
        [SerializeField] private Color color = new(0.35f, 0.9f, 0.55f);
        [SerializeField] private Vector3 scale = Vector3.one;

        private GameObject visual;

        private void Awake()
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(transform, worldPositionStays: false);
            visual.transform.localScale = scale;

            visual.GetComponent<Renderer>().material.color = color;
            visual.AddComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            if (visual != null)
                Destroy(visual);
        }
    }
}
