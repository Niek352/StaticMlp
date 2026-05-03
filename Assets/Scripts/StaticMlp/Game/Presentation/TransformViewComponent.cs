using StaticMlp.Game.EcsViews;
using UnityEngine;

namespace StaticMlp.Game.Presentation
{
    public sealed class TransformViewComponent : MonoBehaviour, IEntityViewPart<ViewTransform>
    {
        public void OnBind(IEntityView view)
        {
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
