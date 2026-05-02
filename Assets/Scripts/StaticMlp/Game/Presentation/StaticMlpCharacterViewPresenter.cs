using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Presentation
{
    public sealed class StaticMlpCharacterViewPresenter : MonoBehaviour
    {
        [Header("Hierarchy")] [SerializeField] private Transform viewRoot;

        [Header("Optional Prefabs")] [SerializeField]
        private GameObject localPlayerPrefab;

        [SerializeField] private GameObject remotePlayerPrefab;
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private bool createPrimitiveFallback = true;

        [Header("Fallback Colors")] [SerializeField]
        private Color localPlayerColor = new(0.1f, 0.55f, 1f);

        [SerializeField] private Color remotePlayerColor = new(0.95f, 0.75f, 0.2f);
        [SerializeField] private Color monsterColor = new(0.9f, 0.15f, 0.2f);

        private readonly Dictionary<EntityGID, Transform> _views = new();
        private readonly List<EntityGID> _deadViews = new();

        private void Awake()
        {
            if (viewRoot == null)
                viewRoot = transform;
        }

        private void Update()
        {
            if (CW.Status != WorldStatus.Initialized)
                return;

            foreach (var e in CW.Query<All<NetworkedTag, CharacterNetState, ViewTransform>>().Entities())
            {
                var view = GetOrCreateView(e);
                ref readonly var viewTransform = ref e.Read<ViewTransform>();
                view.SetPositionAndRotation(viewTransform.RenderPosition, viewTransform.RenderRotation);
            }

            CleanupDeadViews();
        }

        private Transform GetOrCreateView(CW.Entity e)
        {
            if (_views.TryGetValue(e.GID, out var view))
                return view;

            var instance = CreateViewObject(e);
            instance.name = CreateViewName(e);
            instance.transform.SetParent(viewRoot, worldPositionStays: true);
            _views.Add(e.GID, instance.transform);
            return instance.transform;
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

        private void CleanupDeadViews()
        {
            _deadViews.Clear();

            foreach (var pair in _views)
            {
                if (!pair.Key.TryUnpack<ClientCoreWT>(out var e) || !e.Has<NetworkedTag>() || !e.Has<ViewTransform>())
                    _deadViews.Add(pair.Key);
            }

            for (var i = 0; i < _deadViews.Count; i++)
            {
                var gid = _deadViews[i];
                if (_views.TryGetValue(gid, out var view) && view != null)
                    Destroy(view.gameObject);

                _views.Remove(gid);
            }
        }

        private void OnDestroy()
        {
            foreach (var pair in _views)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            }

            _views.Clear();
            _deadViews.Clear();
        }
    }
}