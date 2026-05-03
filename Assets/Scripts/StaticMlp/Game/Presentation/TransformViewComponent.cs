using StaticMlp.Game.EcsViews;
using UnityEngine;

namespace StaticMlp.Game.Presentation
{
    public sealed class TransformViewComponent : MonoBehaviour, IEntityViewPart<ViewTransform>
    {
        public void OnBind(IEntityView view)
        {
            if (!view.Entity.Has<ViewTransform>())
                return;

            ref readonly var viewTransform = ref view.Entity.Read<ViewTransform>();
            Apply(in viewTransform);
        }

        public void OnUnbind()
        {
        }

        public void Apply(in ViewTransform component)
        {
            transform.SetPositionAndRotation(component.RenderPosition, component.RenderRotation);
        }
    }
}
