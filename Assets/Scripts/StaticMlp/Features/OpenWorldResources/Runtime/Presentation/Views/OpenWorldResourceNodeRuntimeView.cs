using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourceNodeRuntimeView : MonoBehaviour, IEntityView
    {
        private IEntityViewPart[] _parts;

        public CW.Entity Entity { get; private set; }

        public static OpenWorldResourceNodeRuntimeView Create(Transform root)
        {
            var go = new GameObject("OpenWorld Resource Node View");
            go.transform.SetParent(root, worldPositionStays: false);
            go.AddComponent<TransformViewComponent>();
            go.AddComponent<OpenWorldResourceNodeViewPart>();

            var view = go.AddComponent<OpenWorldResourceNodeRuntimeView>();
            view.Initialize();
            return view;
        }

        public void Bind(CW.Entity entity)
        {
            Entity = entity;

            for (var i = 0; i < _parts.Length; i++)
                _parts[i].OnBind(this);
        }

        public void Unbind()
        {
            for (var i = 0; i < _parts.Length; i++)
                _parts[i].OnUnbind();

            Entity = default;
        }

        public void Apply<TComponent>(in TComponent component)
            where TComponent : struct, IViewComponent
        {
            for (var i = 0; i < _parts.Length; i++)
                if (_parts[i] is IEntityViewPart<TComponent> typedPart)
                    typedPart.Apply(in component);
        }

        private void Initialize()
        {
            _parts = GetComponentsInChildren<IEntityViewPart>(includeInactive: true);
        }
    }
}
