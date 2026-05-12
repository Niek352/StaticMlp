using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Composition {
    public static class MultiplayerWorldBootstrap {
        public static void CreateServer(WorldConfig config = default, params Assembly[] ecsTypeAssemblies) {
            CreateServer(config, null, ecsTypeAssemblies);
        }

        public static void CreateServer(WorldConfig config, Action registerGeneratedTypes, params Assembly[] ecsTypeAssemblies) {
            SW.Create(config);
            var assemblies = BuildAssemblies(typeof(ServerWT), ecsTypeAssemblies);
            SW.Types().RegisterAll(assemblies.First, assemblies.Rest);
            registerGeneratedTypes?.Invoke();
            SW.Initialize();
            SW.RegisterCluster(1);
        }

        public static void CreateClientCore(WorldConfig config = default, params Assembly[] ecsTypeAssemblies) {
            CreateClientCore(config, null, ecsTypeAssemblies);
        }

        public static void CreateClientCore(WorldConfig config, Action registerGeneratedTypes, params Assembly[] ecsTypeAssemblies) {
            CW.Create(config);
            var assemblies = BuildAssemblies(typeof(ClientCoreWT), ecsTypeAssemblies);
            CW.Types().RegisterAll(assemblies.First, assemblies.Rest);
            registerGeneratedTypes?.Invoke();
            CW.Initialize();
        }

        public static void TickServer() => SW.Tick();
        public static void TickClientCore() => CW.Tick();

        private static AssemblyList BuildAssemblies(Type worldType, Assembly[] ecsTypeAssemblies) {
            var assemblies = new List<Assembly>();
            var seen = new HashSet<Assembly>();
            var loadedAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => x != null)
                .GroupBy(x => x.GetName().Name, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

            AddAssembly(worldType.Assembly, assemblies, seen, loadedAssemblies);

            if (ecsTypeAssemblies != null)
                for (var i = 0; i < ecsTypeAssemblies.Length; i++)
                    AddAssembly(ecsTypeAssemblies[i], assemblies, seen, loadedAssemblies);

            var rest = new Assembly[assemblies.Count - 1];
            for (var i = 1; i < assemblies.Count; i++)
                rest[i - 1] = assemblies[i];

            return new AssemblyList(assemblies[0], rest);
        }

        private static void AddAssembly(
            Assembly assembly,
            List<Assembly> assemblies,
            HashSet<Assembly> seen,
            Dictionary<string, Assembly> loadedAssemblies) {
            if (assembly == null || !seen.Add(assembly))
                return;

            assemblies.Add(assembly);

            var references = assembly.GetReferencedAssemblies();
            for (var i = 0; i < references.Length; i++) {
                var referenceName = references[i].Name;
                if (!ShouldIncludeReferencedAssembly(referenceName))
                    continue;

                if (loadedAssemblies.TryGetValue(referenceName, out var referencedAssembly))
                    AddAssembly(referencedAssembly, assemblies, seen, loadedAssemblies);
            }
        }

        private static bool ShouldIncludeReferencedAssembly(string assemblyName) {
            return assemblyName.StartsWith("StaticMlp.", StringComparison.Ordinal)
                   || assemblyName.StartsWith("Game.Core", StringComparison.Ordinal)
                   || assemblyName.StartsWith("Ecs.Networking", StringComparison.Ordinal)
                   || assemblyName.StartsWith("FFS.StaticEcs", StringComparison.Ordinal)
                   || assemblyName.StartsWith("FFS.StaticPack", StringComparison.Ordinal);
        }

        private readonly struct AssemblyList {
            public readonly Assembly First;
            public readonly Assembly[] Rest;

            public AssemblyList(Assembly first, Assembly[] rest) {
                First = first;
                Rest = rest;
            }
        }
    }
}
