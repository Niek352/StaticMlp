using FFS.Libraries.StaticEcs;
using UnityEditor;
using UnityEngine;

namespace StaticMlp.Features.EcsViews
{
    public readonly struct View : IComponent, ITrackableAdded
    {
        public readonly IEntityView Value;

        public View(IEntityView value)
        {
            Value = value;
        }

        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason)
            where TWorld : struct, IWorldType
        {
            Value.Unbind();

#if UNITY_EDITOR
            if (Value is not MonoBehaviour mono) 
                return;
            
            if (EditorApplication.isPlaying)
                Object.Destroy(mono.gameObject);
            else
                Object.DestroyImmediate(mono.gameObject);
            return;           
#endif
            if (Value is MonoBehaviour monoBehaviour)
                Object.Destroy(monoBehaviour.gameObject);
        }
    }
}
