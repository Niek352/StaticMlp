using System;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.EcsViews
{
    public sealed class ResourcesEntityViewFactory : IEntityViewFactory
    {
        private readonly Transform _root;

        public ResourcesEntityViewFactory(Transform root)
        {
            _root = root;
        }

        public IEntityView CreateViewForEntity(CW.Entity entity, in ViewPath viewPath)
        {
            var prefab = Resources.Load<EntityView>(viewPath.Value);
            if (prefab == null)
                throw new InvalidOperationException($"View prefab not found: {viewPath.Value}");

            var instance = UnityEngine.Object.Instantiate(prefab, _root);
            instance.Bind(entity);
            return instance;
        }

        public void DestroyView(IEntityView view)
        {
            if (view == null)
                return;

            view.Unbind();

            if (view is MonoBehaviour monoBehaviour)
                UnityEngine.Object.Destroy(monoBehaviour.gameObject);
        }
    }
}
