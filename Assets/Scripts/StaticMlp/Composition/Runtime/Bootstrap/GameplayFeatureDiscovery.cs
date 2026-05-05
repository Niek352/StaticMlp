using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;

namespace StaticMlp.Composition
{
    public static class GameplayFeatureDiscovery
    {
        private static IGameplayFeature[] _features;

        public static IReadOnlyList<IGameplayFeature> Features => GetFeatures();

        public static Assembly[] GetEcsTypeAssemblies()
        {
            var assemblies = new List<Assembly>
            {
                typeof(GameplayFeature).Assembly,
                typeof(ViewPath).Assembly
            };

            foreach (var feature in GetFeatures())
                assemblies.Add(feature.GetType().Assembly);

            return assemblies
                .Where(x => x != null)
                .Distinct()
                .OrderBy(x => x.GetName().Name, StringComparer.Ordinal)
                .ToArray();
        }

        public static void RegisterNetworkEvents()
        {
            NetworkEventRegistry.Clear();
            ReplicatedNetworkEventRegistry.RegisterNetworkEvents();
        }

        public static void RegisterReplicationComponents()
        {
            ReplicatedComponentRegistration.RegisterReplicationComponents();
        }

        public static void RegisterPrefabs()
        {
            NetArchetypeRegistry.Clear();

            foreach (var feature in GetFeatures())
                feature.RegisterPrefabs();
        }

        public static void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            foreach (var feature in GetFeatures())
                feature.RegisterServerSystems(systems);
        }

        public static void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            foreach (var feature in GetFeatures())
                feature.RegisterClientCoreSystems(systems);
        }

        public static void RegisterClientViewSync(ViewSyncBuilder views)
        {
            foreach (var feature in GetFeatures())
                feature.RegisterClientViewSync(views);
        }

        private static IGameplayFeature[] GetFeatures()
        {
            if (_features != null)
                return _features;

            _features = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetTypesSafe)
                .Where(IsFeatureType)
                .OrderBy(x => x.FullName, StringComparer.Ordinal)
                .Select(x => (IGameplayFeature)Activator.CreateInstance(x))
                .ToArray();

            return _features;
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(x => x != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsFeatureType(Type type)
        {
            return typeof(IGameplayFeature).IsAssignableFrom(type)
                   && !type.IsAbstract
                   && !type.IsInterface
                   && type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
