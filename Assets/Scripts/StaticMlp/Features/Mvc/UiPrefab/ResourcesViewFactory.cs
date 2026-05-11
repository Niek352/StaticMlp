using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.EcsUi.Mvc
{
    public static class ResourcesViewFactory
    {
        public static ViewFactoryMethod<TView> CreateLazy<TView>(string resourcePath)
            where TView : Component, IView
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ArgumentException("A Resources path is required.", nameof(resourcePath));

            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
                throw new InvalidOperationException($"Resources view prefab was not found at '{resourcePath}'.");

            var viewPrefab = prefab.GetComponent<TView>();
            if (viewPrefab == null)
                throw new InvalidOperationException($"Prefab '{prefab.name}' at '{resourcePath}' must contain {typeof(TView).Name}.");

            return () => Object.Instantiate(viewPrefab);
        }
    }
}
