using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace StaticMlp.Tests.Architecture
{
    public sealed class OpenWorldGenerationArchitectureTests
    {
        private const string FEATURES_ROOT = "Assets/Scripts/StaticMlp/Features";
        private const string OPEN_WORLD_ROOT = FEATURES_ROOT + "/OpenWorldGeneration";
        private const string OPEN_WORLD_RESOURCES_ROOT = FEATURES_ROOT + "/OpenWorldResources";
        private const string FRONTIER_ROOT = FEATURES_ROOT + "/Frontier";
        private const string CONTRACTS_ROOT = OPEN_WORLD_ROOT + "/Runtime/Contracts";
        private const string LPG_NAMESPACE = "Runevision.LayerProcGen";
        private const string OPEN_WORLD_NAMESPACE = "OpenWorldGeneration";

        [Test]
        public void Contracts_DoNotReferenceLayerProcGen()
        {
            var files = EnumerateProjectFiles(CONTRACTS_ROOT, "*.cs", "*.asmdef");
            var offenders = files
                .Where(file => File.ReadAllText(file.FullPath).Contains(LPG_NAMESPACE))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void ExistingFeatures_DoNotReferenceOpenWorldGenerationInV1()
        {
            var offenders = Directory
                .EnumerateDirectories(Path.Combine(ProjectRoot(), FEATURES_ROOT.Replace('/', Path.DirectorySeparatorChar)))
                .Where(directory => !directory.EndsWith("OpenWorldGeneration", StringComparison.Ordinal))
                .Where(directory => !directory.EndsWith("OpenWorldResources", StringComparison.Ordinal))
                .SelectMany(directory => Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
                .Where(path => path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".asmdef", StringComparison.Ordinal))
                .Where(path => File.ReadAllText(path).Contains(OPEN_WORLD_NAMESPACE))
                .Select(path => NormalizeRelativePath(path))
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void OpenWorldResources_DependOnOpenWorldGenerationContractsAndLogicOnly()
        {
            var asmdefPath = Path.Combine(
                ProjectRoot(),
                OPEN_WORLD_RESOURCES_ROOT.Replace('/', Path.DirectorySeparatorChar),
                "Runtime",
                "Logic",
                "StaticMlp.Features.OpenWorldResources.asmdef");
            var text = File.ReadAllText(asmdefPath);

            Assert.That(text, Does.Contain("\"StaticMlp.Features.OpenWorldGeneration.Contracts\""));
            Assert.That(text, Does.Contain("\"StaticMlp.Features.OpenWorldGeneration.Logic\""));
            Assert.That(text, Does.Not.Contain("StaticMlp.Features.OpenWorldGeneration.LayerProcGen"));
            Assert.That(text, Does.Not.Contain("StaticMlp.Features.OpenWorldGeneration.Presentation"));
        }

        [Test]
        public void OpenWorldResources_DoNotReferenceLayerProcGen()
        {
            var files = EnumerateProjectFiles(OPEN_WORLD_RESOURCES_ROOT, "*.cs", "*.asmdef");
            var offenders = files
                .Where(file => File.ReadAllText(file.FullPath).Contains(LPG_NAMESPACE))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void Frontier_DoesNotReferenceOpenWorldGeneration()
        {
            var offenders = EnumerateProjectFiles(FRONTIER_ROOT, "*.cs", "*.asmdef")
                .Where(file => File.ReadAllText(file.FullPath).Contains(OPEN_WORLD_NAMESPACE))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void LayerProcGenReferences_AreRemovedFromRuntimeCode()
        {
            var offenders = EnumerateProjectFiles(FEATURES_ROOT, "*.cs", "*.asmdef")
                .Where(file => File.ReadAllText(file.FullPath).Contains(LPG_NAMESPACE))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void PackageManifest_UsesLayerProcLiteAndNotLayerProcGen()
        {
            var manifestPath = Path.Combine(ProjectRoot(), "Packages", "manifest.json");
            var text = File.ReadAllText(manifestPath);

            Assert.That(text, Does.Contain("com.staticmlp.layer-proc-lite"));
            Assert.That(text, Does.Not.Contain("com.layer-proc-gen"));
        }

        [Test]
        public void LayerProcLitePackage_DoesNotReferenceOpenWorldGenerationFeature()
        {
            var files = EnumerateProjectFiles("Packages/com.staticmlp.layer-proc-lite", "*.cs", "*.asmdef", "*.json");
            var offenders = files
                .Where(file => File.ReadAllText(file.FullPath).Contains("StaticMlp.Features.OpenWorldGeneration"))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void LayerProcLitePackage_DoesNotContainGameSpecificTerrainSemantics()
        {
            var forbiddenTokens = new[]
            {
                "Biome",
                "Water",
                "Wetness",
                "OpenWorld",
                "Resource",
                "Spawn",
                "SurfaceSample",
                "HeightProfile",
                "TerrainMeshGeneration",
                "VisualMesh",
                "PhysicsMesh",
                "NavMeshSource"
            };
            var offenders = EnumerateProjectFiles("Packages/com.staticmlp.layer-proc-lite", "*.cs", "*.asmdef", "*.json")
                .Where(file => forbiddenTokens.Any(token => File.ReadAllText(file.FullPath).Contains(token)))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void LayerProcLiteAsmdef_DependsOnlyOnLowLevelUnityNativeAssemblies()
        {
            var asmdefPath = Path.Combine(ProjectRoot(), "Packages", "com.staticmlp.layer-proc-lite", "Runtime", "StaticMlp.LayerProcLite.asmdef");
            var text = File.ReadAllText(asmdefPath);
            var allowedReferences = new[]
            {
                "\"Unity.Burst\"",
                "\"Unity.Collections\"",
                "\"Unity.Jobs\"",
                "\"Unity.Mathematics\""
            };

            Assert.That(text, Does.Not.Contain("StaticMlp.Features."));
            Assert.That(text, Does.Not.Contain("UnityEngine."));
            Assert.That(text, Does.Not.Contain("\"UnityEngine\""));
            Assert.That(text, Does.Contain("\"noEngineReferences\": false"));
            Assert.That(allowedReferences.All(text.Contains), Is.True);
        }

        [Test]
        public void LayerProcLitePackage_DoesNotUseUnityEngineObjectContracts()
        {
            var forbiddenTokens = new[]
            {
                "UnityEngine",
                "GameObject",
                "NavMeshSurface",
                "Color32",
                "UnityEngine.Bounds"
            };
            var offenders = EnumerateProjectFiles("Packages/com.staticmlp.layer-proc-lite", "*.cs", "*.asmdef")
                .Where(file => forbiddenTokens.Any(token => File.ReadAllText(file.FullPath).Contains(token)))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void OpenWorldGeneration_UsesLayerSchedulersAndGameOwnedSurfaceSamples()
        {
            var jobsRoot = Path.Combine(
                ProjectRoot(),
                OPEN_WORLD_ROOT.Replace('/', Path.DirectorySeparatorChar),
                "Runtime",
                "Logic",
                "Jobs");
            var schedulersRoot = Path.Combine(
                ProjectRoot(),
                OPEN_WORLD_ROOT.Replace('/', Path.DirectorySeparatorChar),
                "Runtime",
                "Logic",
                "LayerSchedulers");
            var heightJob = File.ReadAllText(Path.Combine(jobsRoot, "OpenWorldHeightmapGenerationJob.cs"));
            var surfaceJob = File.ReadAllText(Path.Combine(jobsRoot, "OpenWorldSurfaceSamplingJob.cs"));
            var meshJob = File.ReadAllText(Path.Combine(jobsRoot, "OpenWorldTerrainMeshGenerationJob.cs"));
            var placementJob = File.ReadAllText(Path.Combine(jobsRoot, "ResourcePlacementGenerationJob.cs"));
            var schedulerFiles = Directory.EnumerateFiles(schedulersRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .ToArray();

            Assert.That(heightJob, Does.Contain("LayerProcLitePlanStep"));
            Assert.That(surfaceJob, Does.Contain("LayerProcLitePlanStep"));
            Assert.That(meshJob, Does.Contain("LayerProcLitePlanStep"));
            Assert.That(placementJob, Does.Contain("OpenWorldNativeSurfaceSample"));
            Assert.That(placementJob, Does.Not.Contain("LayerProcLite" + "SurfaceSample"));
            Assert.That(schedulerFiles, Does.Contain("OpenWorldHeightLayerScheduler.cs"));
            Assert.That(schedulerFiles, Does.Contain("OpenWorldSurfaceLayerScheduler.cs"));
            Assert.That(schedulerFiles, Does.Contain("OpenWorldMeshDataLayerScheduler.cs"));
            Assert.That(schedulerFiles, Does.Contain("OpenWorldPlacementLayerScheduler.cs"));
        }

        [Test]
        public void OpenWorldChunkGenerationSystemBase_IsThinLayerRuntimeBridge()
        {
            var systemPath = Path.Combine(
                ProjectRoot(),
                OPEN_WORLD_ROOT.Replace('/', Path.DirectorySeparatorChar),
                "Runtime",
                "Logic",
                "Systems",
                "Shared",
                "OpenWorldChunkGenerationSystemBase.cs");
            var text = File.ReadAllText(systemPath);

            Assert.That(text, Does.Contain("LayerRuntime.AddTopDependency"));
            Assert.That(text, Does.Contain("LayerRuntime.Tick"));
            Assert.That(text, Does.Not.Contain("new OpenWorldHeightmapGenerationJob"));
            Assert.That(text, Does.Not.Contain("new OpenWorldSurfaceSamplingJob"));
            Assert.That(text, Does.Not.Contain("new OpenWorldTerrainMeshGenerationJob"));
            Assert.That(text, Does.Not.Contain("new ResourcePlacementGenerationJob"));
            Assert.That(text, Does.Not.Contain("JobHandle.CombineDependencies"));
        }

        [Test]
        public void OpenWorldGenerationScope_DoesNotContainGeneratedCodeOrPrefabs()
        {
            var roots = new[]
            {
                OPEN_WORLD_ROOT,
                OPEN_WORLD_RESOURCES_ROOT,
                "Assets/Tests/Editor/OpenWorldGeneration",
                "Assets/Tests/Editor/OpenWorldResources",
                "Assets/Tests/Editor/Architecture"
            };
            var offenders = roots
                .Select(root => Path.Combine(ProjectRoot(), root.Replace('/', Path.DirectorySeparatorChar)))
                .Where(Directory.Exists)
                .SelectMany(root => Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                .Where(path => path.EndsWith(".prefab", StringComparison.Ordinal)
                               || path.EndsWith(".Generated.cs", StringComparison.Ordinal))
                .Select(NormalizeRelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        [Test]
        public void SpatialStreaming_DoesNotUseLogicalClusterConstants()
        {
            var forbiddenTokens = new[]
            {
                "NETWORKED_ENTITY_" + "CLUSTER = 1",
                "RegisterCluster" + "(1)",
                "ClientOnly" + "Chunk"
            };
            var offenders = EnumerateProjectFiles("Assets/Scripts/StaticMlp", "*.cs")
                .Where(file => forbiddenTokens.Any(token => File.ReadAllText(file.FullPath).Contains(token)))
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        private static SourceFile[] EnumerateProjectFiles(string relativeRoot, params string[] patterns)
        {
            var root = Path.Combine(ProjectRoot(), relativeRoot.Replace('/', Path.DirectorySeparatorChar));
            return patterns
                .SelectMany(pattern => Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
                .Select(path => new SourceFile(path, NormalizeRelativePath(path)))
                .ToArray();
        }

        private static string ProjectRoot()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                var featuresPath = Path.Combine(directory.FullName, "Assets", "Scripts", "StaticMlp", "Features");
                if (Directory.Exists(featuresPath))
                    return directory.FullName;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not resolve project root.");
        }

        private static string NormalizeRelativePath(string fullPath)
        {
            var root = ProjectRoot().Replace('\\', '/').TrimEnd('/') + "/";
            var normalized = Path.GetFullPath(fullPath).Replace('\\', '/');
            return normalized.Substring(root.Length);
        }

        private readonly struct SourceFile
        {
            public SourceFile(string fullPath, string relativePath)
            {
                FullPath = fullPath;
                RelativePath = relativePath;
            }

            public readonly string FullPath;
            public readonly string RelativePath;
        }
    }
}
