using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.EcsViews
{
    public sealed class EntityView : MonoBehaviour, IEntityView
    {
        private IEntityViewPart[] _parts;

        public CW.Entity Entity { get; private set; }

        private void Awake()
        {
            _parts = GetComponentsInChildren<IEntityViewPart>(includeInactive: true);
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
    }
}
