using System;
using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Editor.Ai
{
    public static class AiEditorDebugCalculator
    {
        public static AiBotDebugSnapshot BuildSnapshot(
            SW.Entity entity,
            AiActionCatalog catalog,
            in AiBehaviorDefinition behavior,
            in AiBrain brain,
            in AiTaskState taskState)
        {
            var tasks = behavior.Tasks ?? Array.Empty<UtilityTaskDefinition>();
            var rows = new AiTaskDebugRow[tasks.Length];
            var bestTaskIndex = -1;
            var bestScore = 0f;

            for (var i = 0; i < tasks.Length; i++)
            {
                ref readonly var task = ref tasks[i];
                var considerations = BuildConsiderationRows(entity, catalog, task.Considerations, out var score);
                rows[i] = new AiTaskDebugRow
                {
                    TaskType = task.Task,
                    Score = score,
                    IsSelectedTask = task.Task == taskState.Task,
                    IsActiveTask = taskState.HasActiveTask && task.Task == taskState.ActiveTask,
                    Considerations = considerations
                };

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTaskIndex = i;
                }
            }

            if (bestTaskIndex >= 0)
                rows[bestTaskIndex].IsBestTask = true;

            return new AiBotDebugSnapshot
            {
                BehaviorId = brain.BehaviorId,
                Position = entity.Read<CharacterNetState>().Position,
                CurrentTask = brain.CurrentTask,
                SelectedTask = taskState.Task,
                ActiveTask = taskState.ActiveTask,
                HasActiveTask = taskState.HasActiveTask,
                Tasks = rows,
                BestTaskIndex = bestTaskIndex
            };
        }

        private static AiConsiderationDebugRow[] BuildConsiderationRows(
            SW.Entity entity,
            AiActionCatalog catalog,
            UtilityConsideration[] considerations,
            out float score)
        {
            if (considerations == null || considerations.Length == 0)
            {
                score = 0f;
                return Array.Empty<AiConsiderationDebugRow>();
            }

            score = 1f;
            var rows = new AiConsiderationDebugRow[considerations.Length];
            for (var i = 0; i < considerations.Length; i++)
            {
                var consideration = considerations[i];
                var rawValue = catalog.ReadUtilityValue(entity, consideration.VariableId);
                var curvedValue = AiUtilityEvaluator.ApplyCurve(rawValue, consideration.Curve);
                var factor = AiUtilityEvaluator.EvaluateConsiderationFactor(
                    rawValue,
                    consideration.Curve,
                    consideration.Weight);

                score *= factor;
                rows[i] = new AiConsiderationDebugRow
                {
                    VariableId = consideration.VariableId,
                    VariableName = AiEditorUtilityBindingNameCatalog.GetVariableName(consideration.VariableId),
                    Curve = consideration.Curve,
                    RawValue = rawValue,
                    CurvedValue = curvedValue,
                    Weight = consideration.Weight,
                    Factor = factor
                };
            }

            score = Mathf.Clamp01(score);
            return rows;
        }
    }
}
