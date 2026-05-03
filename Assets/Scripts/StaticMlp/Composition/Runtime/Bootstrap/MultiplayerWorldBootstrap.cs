using System;
using System.Collections.Generic;
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

            AddAssembly(worldType.Assembly, assemblies, seen);

            if (ecsTypeAssemblies != null)
                for (var i = 0; i < ecsTypeAssemblies.Length; i++)
                    AddAssembly(ecsTypeAssemblies[i], assemblies, seen);

            var rest = new Assembly[assemblies.Count - 1];
            for (var i = 1; i < assemblies.Count; i++)
                rest[i - 1] = assemblies[i];

            return new AssemblyList(assemblies[0], rest);
        }

        private static void AddAssembly(Assembly assembly, List<Assembly> assemblies, HashSet<Assembly> seen) {
            if (assembly == null || !seen.Add(assembly))
                return;

            assemblies.Add(assembly);
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
