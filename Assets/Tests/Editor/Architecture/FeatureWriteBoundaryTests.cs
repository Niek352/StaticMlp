using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace StaticMlp.Tests.Architecture
{
    public sealed class FeatureWriteBoundaryTests
    {
        private const string FEATURES_ROOT = "Assets/Scripts/StaticMlp/Features";
        private const string BOUNDARY_INSTRUCTION =
            "write into another feature through `StaticEcs.Event`; owner feature system applies `Mut<T>()`.";

        private static readonly Regex TypeDeclarationRegex = new Regex(
            @"(?m)^\s*(?:public|internal|private|protected)?\s*(?:readonly\s+|sealed\s+|abstract\s+|static\s+|partial\s+)*\b(?:struct|class|interface|enum)\s+(?<type>[A-Za-z_][A-Za-z0-9_]*)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex QualifiedMutPrefixRegex = new Regex(
            @"(?:ReplicationMut|ClientProjection)\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly WriteApiPattern[] WriteApiPatterns =
        {
            new WriteApiPattern(
                "ReplicationMut.Mut",
                new Regex(
                    @"\bReplicationMut\s*\.\s*Mut\s*<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant)),
            new WriteApiPattern(
                "ClientProjection.Mut",
                new Regex(
                    @"\bClientProjection\s*\.\s*Mut\s*<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant)),
            new WriteApiPattern(
                "Mut",
                new Regex(
                    @"\.\s*Mut\s*<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant))
        };

        private static readonly Violation[] Baseline =
        {
            new Violation("Mut", "Assets/Scripts/StaticMlp/Features/AiTaskExecution/Runtime/Systems/Server/ServerAiTaskExecutionSystem.cs", 17, "AiTaskState", "AiTaskExecution", "AiBots"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Factories/ConstructionSiteFactory.cs", 58, "ConstructionSiteState", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Factories/ConstructionSiteFactory.cs", 59, "ConstructionResources", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Factories/ConstructionSiteFactory.cs", 68, "ConstructionSiteState", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Factories/ConstructionSiteFactory.cs", 70, "ConstructionProgress", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/BuildConstructionHandler.cs", 37, "ConstructionSiteState", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/BuildConstructionHandler.cs", 39, "ConstructionProgress", "Buildings", "Settlement"),
            new Violation("ClientProjection.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/BuildConstructionProjector.cs", 27, "ConstructionSiteState", "Buildings", "Settlement"),
            new Violation("ClientProjection.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/BuildConstructionProjector.cs", 28, "ConstructionProgress", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesHandler.cs", 52, "SettlementSharedResources", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesHandler.cs", 56, "ConstructionSiteState", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesHandler.cs", 57, "ConstructionResources", "Buildings", "Settlement"),
            new Violation("ClientProjection.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesProjector.cs", 34, "SettlementSharedResources", "Buildings", "Settlement"),
            new Violation("ClientProjection.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesProjector.cs", 38, "ConstructionSiteState", "Buildings", "Settlement"),
            new Violation("ClientProjection.Mut", "Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesProjector.cs", 39, "ConstructionResources", "Buildings", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerDamageApplySystem.cs", 52, "Health", "Combat", "Shared"),
            new Violation("Mut", "Assets/Scripts/StaticMlp/Features/Effects/Runtime/Presentation/Systems/Client/ClientCombatVisualSpawnSystem.cs", 31, "LocalCombatPredictionState", "Effects", "Combat"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1BossPreparationProgressionSystem.cs", 46, "BossLoadoutPreparationState", "Progression", "Build"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1BossPreparationProgressionSystem.cs", 53, "BossPreparedLoadoutSnapshot", "Progression", "Build"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1RewardApplicationSystem.cs", 37, "SettlementSharedResources", "Progression", "Settlement"),
            new Violation("Mut", "Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerTaskSyncSystem.cs", 17, "AiTaskState", "Settlement.Workers", "AiBots"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Logic/Systems/Server/ServerStage1FlowSystem.cs", 52, "Stage1SettlementProgression", "Stage1", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Logic/Systems/Server/ServerStage1FlowSystem.cs", 61, "Stage1SettlementProgression", "Stage1", "Settlement"),
            new Violation("ReplicationMut.Mut", "Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Logic/Systems/Server/ServerStage1FlowSystem.cs", 79, "Stage1SettlementProgression", "Stage1", "Settlement")
        };

        [Test]
        public void CrossFeatureMutWritesStayWithinBaseline()
        {
            var actual = new HashSet<Violation>(ScanCrossFeatureWrites());
            var baseline = new HashSet<Violation>(Baseline);

            var unexpected = actual
                .Except(baseline)
                .OrderBy(v => v.CallerFile, StringComparer.Ordinal)
                .ThenBy(v => v.Line)
                .ThenBy(v => v.Api, StringComparer.Ordinal)
                .ToArray();
            var stale = baseline
                .Except(actual)
                .OrderBy(v => v.CallerFile, StringComparer.Ordinal)
                .ThenBy(v => v.Line)
                .ThenBy(v => v.Api, StringComparer.Ordinal)
                .ToArray();

            var report = BuildMarkdownReport(actual, baseline, unexpected, stale);
            TestContext.Out.WriteLine(report);

            if (unexpected.Length == 0 && stale.Length == 0)
                return;

            Assert.Fail(report);
        }

        private static IEnumerable<Violation> ScanCrossFeatureWrites()
        {
            var projectRoot = ResolveProjectRoot();
            var featuresRoot = Path.Combine(projectRoot, "Assets", "Scripts", "StaticMlp", "Features");
            var files = Directory
                .EnumerateFiles(featuresRoot, "*.cs", SearchOption.AllDirectories)
                .Select(path => new SourceFile(path, NormalizeRelativePath(projectRoot, path)))
                .Where(file => IsSourceFileIncluded(file.RelativePath))
                .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
                .ToArray();

            var typeOwners = BuildTypeOwnerMap(files);

            foreach (var file in files)
            {
                var text = File.ReadAllText(file.FullPath);
                var callerFeature = GetFeatureName(file.RelativePath);

                foreach (var pattern in WriteApiPatterns)
                {
                    foreach (Match match in pattern.Regex.Matches(text))
                    {
                        if (pattern.Api == "Mut" && IsQualifiedMutCall(text, match.Index))
                            continue;

                        if (IsCommentLine(text, match.Index))
                            continue;

                        var typeName = match.Groups["type"].Value;
                        if (!typeOwners.TryGetValue(typeName, out var ownerFeature)
                            || ownerFeature.Length == 0
                            || ownerFeature == callerFeature)
                            continue;

                        yield return new Violation(
                            pattern.Api,
                            file.RelativePath,
                            GetLineNumber(text, match.Index),
                            typeName,
                            callerFeature,
                            ownerFeature);
                    }
                }
            }
        }

        private static Dictionary<string, string> BuildTypeOwnerMap(IEnumerable<SourceFile> files)
        {
            var typeOwners = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var file in files)
            {
                var featureName = GetFeatureName(file.RelativePath);
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

            throw new DirectoryNotFoundException($"Could not resolve project root containing {FEATURES_ROOT}.");
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
                throw new InvalidOperationException($"Path {fullPath} is outside project root {projectRoot}.");

            return normalizedFullPath.Substring(normalizedRoot.Length);
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        private static string GetFeatureName(string relativePath)
        {
            var prefix = FEATURES_ROOT + "/";
            if (!relativePath.StartsWith(prefix, StringComparison.Ordinal))
                throw new InvalidOperationException($"Path {relativePath} is outside {FEATURES_ROOT}.");

            var featureStart = prefix.Length;
            var featureEnd = relativePath.IndexOf('/', featureStart);
            return featureEnd < 0
                ? relativePath.Substring(featureStart)
                : relativePath.Substring(featureStart, featureEnd - featureStart);
        }

        private static bool IsQualifiedMutCall(string text, int dotIndex)
        {
            var prefixStart = Math.Max(0, dotIndex - 32);
            var prefix = text.Substring(prefixStart, dotIndex - prefixStart);
            return QualifiedMutPrefixRegex.IsMatch(prefix);
        }

        private static bool IsCommentLine(string text, int index)
        {
            var lineStart = index;
            while (lineStart > 0 && text[lineStart - 1] != '\n')
                lineStart--;

            while (lineStart < index && char.IsWhiteSpace(text[lineStart]))
                lineStart++;

            return lineStart + 1 < text.Length
                   && text[lineStart] == '/'
                   && text[lineStart + 1] == '/';
        }

        private static int GetLineNumber(string text, int index)
        {
            var line = 1;
            for (var i = 0; i < index; i++)
            {
                if (text[i] == '\n')
                    line++;
            }

            return line;
        }

        private static void AppendViolations(StringBuilder message, IEnumerable<Violation> violations)
        {
            foreach (var violation in violations)
                message.AppendLine("- " + violation);
        }

        private static string BuildMarkdownReport(
            HashSet<Violation> actual,
            HashSet<Violation> baseline,
            Violation[] unexpected,
            Violation[] stale)
        {
            var message = new StringBuilder();
            message.AppendLine("# Feature Write Boundary Report");
            message.AppendLine();
            message.AppendLine("Copy this markdown into Codex when this architecture guard needs migration work.");
            message.AppendLine();
            message.AppendLine("## Status");
            message.AppendLine();
            message.AppendLine($"- Actual cross-feature mutable writes: `{actual.Count}`");
            message.AppendLine($"- Baseline cross-feature mutable writes: `{baseline.Count}`");
            message.AppendLine($"- New writes outside baseline: `{unexpected.Length}`");
            message.AppendLine($"- Stale baseline entries: `{stale.Length}`");
            message.AppendLine($"- Rule: {BOUNDARY_INSTRUCTION}");

            if (unexpected.Length == 0 && stale.Length == 0)
            {
                message.AppendLine();
                message.AppendLine("No baseline drift detected. Existing entries are known architecture debt.");
            }

            if (unexpected.Length > 0)
            {
                message.AppendLine();
                message.AppendLine("## New Cross-Feature Writes");
                message.AppendLine();
                AppendMarkdownTable(message, unexpected);
            }

            if (stale.Length > 0)
            {
                message.AppendLine();
                message.AppendLine("## Stale Baseline Entries");
                message.AppendLine();
                AppendMarkdownTable(message, stale);
            }

            message.AppendLine();
            message.AppendLine("## Codex Prompt");
            message.AppendLine();
            message.AppendLine("```md");
            message.AppendLine("Fix the StaticMlp feature write boundary architecture guard.");
            message.AppendLine();
            message.AppendLine("Rules:");
            message.AppendLine("- Do not let one feature write another feature's state through `Mut<T>()`, `ReplicationMut.Mut<T>()`, or `ClientProjection.Mut<T>()`.");
            message.AppendLine("- Cross-feature writes must go through `StaticEcs.Event`; the owner feature system applies `Mut<T>()`.");
            message.AppendLine("- If an entry is stale because the code was migrated, remove only that baseline entry from `FeatureWriteBoundaryTests`.");
            message.AppendLine("- Do not refactor unrelated feature code.");

            if (unexpected.Length > 0)
            {
                message.AppendLine();
                message.AppendLine("New writes to migrate:");
                AppendPromptList(message, unexpected);
            }

            if (stale.Length > 0)
            {
                message.AppendLine();
                message.AppendLine("Stale baseline entries to remove:");
                AppendPromptList(message, stale);
            }

            if (unexpected.Length == 0 && stale.Length == 0)
            {
                message.AppendLine();
                message.AppendLine("There is no baseline drift. Pick the largest existing debt edge and migrate it through an owner-feature event pipeline.");
            }

            message.AppendLine("```");
            message.AppendLine();
            message.AppendLine("## Current Baseline");
            message.AppendLine();
            AppendMarkdownTable(
                message,
                baseline
                    .OrderBy(v => v.CallerFile, StringComparer.Ordinal)
                    .ThenBy(v => v.Line)
                    .ThenBy(v => v.Api, StringComparer.Ordinal));

            return message.ToString();
        }

        private static void AppendMarkdownTable(StringBuilder message, IEnumerable<Violation> violations)
        {
            message.AppendLine("| File | Line | API | Type | From | Owner |");
            message.AppendLine("| --- | ---: | --- | --- | --- | --- |");

            foreach (var violation in violations)
            {
                message.Append("| `");
                message.Append(violation.CallerFile);
                message.Append("` | ");
                message.Append(violation.Line);
                message.Append(" | `");
                message.Append(violation.Api);
                message.Append("` | `");
                message.Append(violation.TypeName);
                message.Append("` | `");
                message.Append(violation.CallerFeature);
                message.Append("` | `");
                message.Append(violation.OwnerFeature);
                message.AppendLine("` |");
            }
        }

        private static void AppendPromptList(StringBuilder message, IEnumerable<Violation> violations)
        {
            foreach (var violation in violations)
                message.AppendLine($"- `{violation}`");
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

        private readonly struct WriteApiPattern
        {
            public WriteApiPattern(string api, Regex regex)
            {
                Api = api;
                Regex = regex;
            }

            public readonly string Api;
            public readonly Regex Regex;
        }

        private readonly struct Violation : IEquatable<Violation>
        {
            public Violation(
                string api,
                string callerFile,
                int line,
                string typeName,
                string callerFeature,
                string ownerFeature)
            {
                Api = api;
                CallerFile = callerFile;
                Line = line;
                TypeName = typeName;
                CallerFeature = callerFeature;
                OwnerFeature = ownerFeature;
            }

            public readonly string Api;
            public readonly string CallerFile;
            public readonly int Line;
            public readonly string TypeName;
            public readonly string CallerFeature;
            public readonly string OwnerFeature;

            public bool Equals(Violation other)
            {
                return Api == other.Api
                       && CallerFile == other.CallerFile
                       && Line == other.Line
                       && TypeName == other.TypeName
                       && CallerFeature == other.CallerFeature
                       && OwnerFeature == other.OwnerFeature;
            }

            public override bool Equals(object obj)
            {
                return obj is Violation other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Api);
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(CallerFile);
                    hash = hash * 31 + Line;
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(TypeName);
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(CallerFeature);
                    hash = hash * 31 + StringComparer.Ordinal.GetHashCode(OwnerFeature);
                    return hash;
                }
            }

            public override string ToString()
            {
                return $"{CallerFile}:{Line} {Api}<{TypeName}> from {CallerFeature} to {OwnerFeature}";
            }
        }
    }
}
