using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace StaticMlp.Tests.Architecture
{
    public sealed class PresentationDependencyScopeTests
    {
        private const string SETTLEMENT_PRESENTATION_ROOT = "Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation";
        private const string SETTLEMENT_PRESENTATION_ASMDEF = "Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/StaticMlp.Features.Settlement.Presentation.asmdef";

        private static readonly Regex ClientProjectionReadRegex = new Regex(
            @"\bClientProjection\s*\.\s*Read\s*<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex EntityReadRegex = new Regex(
            @"\.\s*Read\s*<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex TypeDeclarationRegex = new Regex(
            @"(?m)^\s*(?:public|internal|private|protected)?\s*(?:readonly\s+|sealed\s+|abstract\s+|static\s+|partial\s+)*\b(?:struct|class|interface|enum)\s+(?<type>[A-Za-z_][A-Za-z0-9_]*)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly string[] ForeignLogicFeatures =
        {
            "Progression",
            "Loadout",
            "Frontier",
            "Threat",
            "Raid",
            "Boss",
            "Buildings",
            "BuildingCatalog",
            "AiBots"
        };

        private static readonly ReadViolation[] BaselineReads =
        {
            new ReadViolation("ClientProjection.Read", "Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingContextPanelPresentation.cs", "ConstructionSiteState", "Buildings"),
            new ReadViolation("ClientProjection.Read", "Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingContextPanelPresentation.cs", "ConstructionProgress", "Buildings"),
        };

        private static readonly string[] ApprovedAsmdefLogicReferences =
        {
            "StaticMlp.Features.Buildings",
            "StaticMlp.Features.BuildingCatalog",
            "StaticMlp.Features.Loadout.Logic",
            "StaticMlp.Features.Frontier.Logic",
            "StaticMlp.Features.Progression.Logic",
            "StaticMlp.Features.AiBots",
            "StaticMlp.Features.Settlement.Workers.Logic",
        };

        [Test]
        public void SettlementPresentation_DoesNotReadForeignLogicComponentsBeyondBaseline()
        {
            var projectRoot = ResolveProjectRoot();
            var presentationRoot = Path.Combine(projectRoot, SETTLEMENT_PRESENTATION_ROOT);
            var files = Directory
                .EnumerateFiles(presentationRoot, "*.cs", SearchOption.AllDirectories)
                .Select(path => new SourceFile(path, NormalizeRelativePath(projectRoot, path)))
                .Where(file => IsSourceFileIncluded(file.RelativePath))
                .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
                .ToArray();

            var typeOwners = BuildTypeOwnerMap(
                Directory.EnumerateFiles(Path.Combine(projectRoot, "Assets", "Scripts", "StaticMlp", "Features"), "*.cs", SearchOption.AllDirectories)
                    .Select(path => new SourceFile(path, NormalizeRelativePath(projectRoot, path)))
                    .Where(file => IsSourceFileIncluded(file.RelativePath)));

            var actual = new HashSet<ReadViolation>(ScanPresentationReads(files, typeOwners));
            var baseline = new HashSet<ReadViolation>(BaselineReads);

            var unexpected = actual.Except(baseline).OrderBy(v => v.File).ThenBy(v => v.TypeName).ToArray();
            var stale = baseline.Except(actual).OrderBy(v => v.File).ThenBy(v => v.TypeName).ToArray();

            if (unexpected.Length > 0 || stale.Length > 0)
            {
                var report = new StringBuilder();
                report.AppendLine("# Presentation Read Boundary Report");
                if (unexpected.Length > 0)
                {
                    report.AppendLine("## New foreign logic reads (fail):");
                    foreach (var v in unexpected)
                        report.AppendLine("- " + v);
                }
                if (stale.Length > 0)
                {
                    report.AppendLine("## Stale baseline reads (update test):");
                    foreach (var v in stale)
                        report.AppendLine("- " + v);
                }
                TestContext.Out.WriteLine(report.ToString());
            }

            Assert.That(unexpected, Is.Empty, "New foreign logic reads detected in Settlement.Presentation.");
        }

        [Test]
        public void MonolithicHudState_IsRemoved()
        {
            var projectRoot = ResolveProjectRoot();
            var hudStatePath = Path.Combine(projectRoot, SETTLEMENT_PRESENTATION_ROOT, "Stage1HudState.cs");
            Assert.That(File.Exists(hudStatePath), Is.False,
                "Monolithic Stage1HudState must be removed. Use owner-scoped presentation resources instead.");
        }

        [Test]
        public void PresentationAsmdef_DoesNotAddNewForeignLogicDependencies()
        {
            var projectRoot = ResolveProjectRoot();
            var asmdefPath = Path.Combine(projectRoot, SETTLEMENT_PRESENTATION_ASMDEF);
            var text = File.ReadAllText(asmdefPath);

            var logicRefs = new List<string>();
            var matches = Regex.Matches(text, "\"StaticMlp\\.Features\\.[^\"]*\\.Logic\"");
            foreach (Match m in matches)
                logicRefs.Add(m.Value.Trim('"'));

            var extra = logicRefs.Except(ApprovedAsmdefLogicReferences, StringComparer.Ordinal).ToArray();
            Assert.That(extra, Is.Empty,
                "Settlement.Presentation asmdef added new foreign *.Logic dependencies: " + string.Join(", ", extra) + ".");
        }

        private static IEnumerable<ReadViolation> ScanPresentationReads(IEnumerable<SourceFile> files, Dictionary<string, string> typeOwners)
        {
            foreach (var file in files)
            {
                var text = File.ReadAllText(file.FullPath);

                foreach (Match match in ClientProjectionReadRegex.Matches(text))
                {
                    var typeName = match.Groups["type"].Value;
                    if (!typeOwners.TryGetValue(typeName, out var ownerFeature) || string.IsNullOrEmpty(ownerFeature))
                        continue;
                    if (ownerFeature == "Settlement" || ownerFeature == "Settlement.Workers")
                        continue;
                    if (!ForeignLogicFeatures.Contains(ownerFeature))
                        continue;

                    yield return new ReadViolation(
                        "ClientProjection.Read",
                        file.RelativePath,
                        typeName,
                        ownerFeature);
                }

                foreach (Match match in EntityReadRegex.Matches(text))
                {
                    var prefixStart = Math.Max(0, match.Index - 32);
                    var prefix = text.Substring(prefixStart, match.Index - prefixStart);
                    if (prefix.Contains("ClientProjection"))
                        continue;

                    var typeName = match.Groups["type"].Value;
                    if (!typeOwners.TryGetValue(typeName, out var ownerFeature) || string.IsNullOrEmpty(ownerFeature))
                        continue;
                    if (ownerFeature == "Settlement" || ownerFeature == "Settlement.Workers")
                        continue;
                    if (!ForeignLogicFeatures.Contains(ownerFeature))
                        continue;

                    yield return new ReadViolation(
                        "entity.Read",
                        file.RelativePath,
                        typeName,
                        ownerFeature);
                }
            }
        }

        private static Dictionary<string, string> BuildTypeOwnerMap(IEnumerable<SourceFile> files)
        {
            var typeOwners = new Dictionary<string, string>(StringComparer.Ordinal);
            var featuresRoot = "Assets/Scripts/StaticMlp/Features";

            foreach (var file in files)
            {
                var featureName = GetFeatureName(file.RelativePath, featuresRoot);
                if (featureName == null)
                    continue;

                var text = File.ReadAllText(file.FullPath);
                foreach (Match match in TypeDeclarationRegex.Matches(text))
                {
                    var typeName = match.Groups["type"].Value;
                    if (typeOwners.TryGetValue(typeName, out var existingOwner))
                    {
                        if (existingOwner != featureName)
                            typeOwners[typeName] = string.Empty;
                        continue;
                    }
                    typeOwners.Add(typeName, featureName);
                }
            }

            return typeOwners;
        }

        private static string GetFeatureName(string relativePath, string featuresRoot)
        {
            var prefix = featuresRoot + "/";
            if (!relativePath.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            var featureStart = prefix.Length;
            var featureEnd = relativePath.IndexOf('/', featureStart);
            return featureEnd < 0
                ? relativePath.Substring(featureStart)
                : relativePath.Substring(featureStart, featureEnd - featureStart);
        }

        private static string ResolveProjectRoot()
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

        private static bool IsSourceFileIncluded(string relativePath)
        {
            return !relativePath.EndsWith(".Generated.cs", StringComparison.Ordinal)
                   && relativePath.IndexOf("/Generated/", StringComparison.Ordinal) < 0;
        }

        private static string NormalizeRelativePath(string projectRoot, string fullPath)
        {
            var normalizedRoot = NormalizePath(Path.GetFullPath(projectRoot)).TrimEnd('/') + "/";
            var normalizedFullPath = NormalizePath(Path.GetFullPath(fullPath));
            if (!normalizedFullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Path " + fullPath + " is outside project root.");
            return normalizedFullPath.Substring(normalizedRoot.Length);
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
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

        private readonly struct ReadViolation : IEquatable<ReadViolation>
        {
            public ReadViolation(string api, string file, string typeName, string ownerFeature)
            {
                Api = api;
                File = file;
                TypeName = typeName;
                OwnerFeature = ownerFeature;
            }

            public readonly string Api;
            public readonly string File;
            public readonly string TypeName;
            public readonly string OwnerFeature;

            public bool Equals(ReadViolation other)
            {
                return Api == other.Api && File == other.File && TypeName == other.TypeName && OwnerFeature == other.OwnerFeature;
            }

            public override bool Equals(object obj) => obj is ReadViolation other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Api);
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(File);
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(TypeName);
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(OwnerFeature);
                    return hash;
                }
            }

            public override string ToString()
            {
                return File + ": " + Api + "<" + TypeName + "> from Settlement.Presentation to " + OwnerFeature;
            }
        }
    }
}
