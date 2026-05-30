using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aspid.StaticEcs.Windows
{
    public static class EcsResourcesWindowShellViewFactory
    {
        public static EcsWindowShellViewFactoryMethod<TView> CreateLazy<TView>(string resourcePath)
            where TView : Component, IEcsWindowShellView
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ArgumentException("A Resources path is required.", nameof(resourcePath));

            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
                throw new InvalidOperationException($"Resources window prefab was not found at '{resourcePath}'.");

            var shellPrefab = prefab.GetComponent<TView>();
            if (shellPrefab == null)
                throw new InvalidOperationException($"Prefab '{prefab.name}' at '{resourcePath}' must contain {typeof(TView).Name}.");

            return () => Object.Instantiate(shellPrefab);
        }
    }
}
