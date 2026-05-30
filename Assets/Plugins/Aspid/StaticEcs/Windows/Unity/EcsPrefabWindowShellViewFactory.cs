using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aspid.StaticEcs.Windows
{
    public static class EcsPrefabWindowShellViewFactory
    {
        public static EcsWindowShellViewFactoryMethod<TView> CreateLazy<TView>(TView prefab, Transform parent)
            where TView : Component, IEcsWindowShellView
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            return () => Object.Instantiate(prefab, parent, false);
        }

        public static EcsWindowShellViewFactoryMethod<TView> Preallocate<TView>(TView prefab, Transform parent)
            where TView : Component, IEcsWindowShellView
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var cachedInstance = Object.Instantiate(prefab, parent, false);
            cachedInstance.HideAsync(CancellationToken.None, true).Forget();
            return () => cachedInstance;
        }
    }
}
