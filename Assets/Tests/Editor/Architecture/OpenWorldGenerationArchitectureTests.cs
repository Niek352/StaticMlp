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
        public void LayerProcGenReferences_AreLimitedToFutureAdapterFolder()
        {
            var offenders = EnumerateProjectFiles(FEATURES_ROOT, "*.cs", "*.asmdef")
                .Where(file => File.ReadAllText(file.FullPath).Contains(LPG_NAMESPACE))
                .Where(file => file.RelativePath.IndexOf("/Runtime/Logic/LayerProcGen/", StringComparison.Ordinal) < 0)
                .Select(file => file.RelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
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
