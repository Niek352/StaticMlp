using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Game.Presentation
{
    public sealed class CharacterView : MonoBehaviour, IEntityViewPart
    {
        [Header("Optional Visual Prefabs")] [SerializeField]
        private GameObject localPlayerPrefab;

        [SerializeField] private GameObject remotePlayerPrefab;
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private bool createPrimitiveFallback = true;

        [Header("Fallback Colors")] [SerializeField]
        private Color localPlayerColor = new(0.1f, 0.55f, 1f);

        [SerializeField] private Color remotePlayerColor = new(0.95f, 0.75f, 0.2f);
        [SerializeField] private Color monsterColor = new(0.9f, 0.15f, 0.2f);

        private GameObject _visual;

        public void OnBind(IEntityView view)
        {
            DestroyVisual();
            _visual = CreateViewObject(view.Entity);
            _visual.name = CreateViewName(view.Entity);
            _visual.transform.SetParent(transform, worldPositionStays: false);
        }

        private GameObject CreateViewObject(CW.Entity e)
        {
            var prefab = SelectPrefab(e);
            if (prefab != null)
                return Instantiate(prefab);

            if (!createPrimitiveFallback)
                throw new System.InvalidOperationException(
                    "Character view prefab is not assigned and primitive fallback is disabled.");

            var primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            primitive.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            primitive.GetComponent<Renderer>().material.color = SelectFallbackColor(e);
            Object.Destroy(primitive.GetComponent<Collider>());
            return primitive;
        }

        private GameObject SelectPrefab(CW.Entity e)
        {
            if (e.Has<PlayerTag>())
                return e.Has<LocalOwned>() ? localPlayerPrefab : remotePlayerPrefab;

            if (e.Has<MonsterTag>())
                return monsterPrefab;

            return null;
        }

        private Color SelectFallbackColor(CW.Entity e)
        {
            if (e.Has<PlayerTag>())
                return e.Has<LocalOwned>() ? localPlayerColor : remotePlayerColor;

            if (e.Has<MonsterTag>())
                return monsterColor;

            return Color.white;
        }

        private static string CreateViewName(CW.Entity e)
        {
            if (e.Has<PlayerTag>())
                return e.Has<LocalOwned>() ? "Local Player View" : "Remote Player View";

            if (e.Has<MonsterTag>())
                return "Monster View";

            return "Networked Character View";
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
            if (_visual == null)
                return;

            Destroy(_visual);
            _visual = null;
        }
    }
}
