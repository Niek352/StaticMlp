using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.EcsUi.Mvc
{
    public static class PrefabViewFactory
    {
        public static ViewFactoryMethod<TView> CreateLazy<TView>(TView prefab, Transform parent)
            where TView : Component, IView
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            return () => UnityEngine.Object.Instantiate(prefab, parent, false);
        }

        public static ViewFactoryMethod<TView> Preallocate<TView>(TView prefab, Transform parent)
            where TView : Component, IView
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var cachedInstance = UnityEngine.Object.Instantiate(prefab, parent, false);
            cachedInstance.HideAsync(CancellationToken.None, true).Forget();
            return () => cachedInstance;
        }
    }
}
