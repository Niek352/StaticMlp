using UnityEngine;

namespace StaticMlp.Game.EcsViews
{
    public sealed class ViewRootProvider : MonoBehaviour
    {
        [SerializeField] private Transform _root;

        public static Transform Root { get; private set; }

        private void Awake()
        {
            Root = _root != null ? _root : transform;
        }

        private void OnDestroy()
        {
            if (Root == _root || Root == transform)
                Root = null;
        }
    }
}
