using System;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public static class AiUtilityEvaluator
    {
        public static AiTaskType SelectBestTask(SW.Entity entity, AiActionCatalog catalog, in AiBehaviorDefinition behavior)
        {
            var bestScore = 0f;
            var bestTask = AiTaskType.Idle;

            for (var i = 0; i < behavior.Tasks.Length; i++)
            {
                ref readonly var task = ref behavior.Tasks[i];
                var score = EvaluateTask(entity, catalog, task.Considerations);
                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestTask = task.Task;
            }

            return bestTask;
        }

        public static float EvaluateTask(SW.Entity entity, AiActionCatalog catalog, UtilityConsideration[] considerations)
        {
            var score = 1f;
            for (var i = 0; i < considerations.Length; i++)
            {
                ref readonly var consideration = ref considerations[i];
                var value = catalog.ReadUtilityValue(entity, consideration.VariableId);
                var factor = EvaluateConsiderationFactor(value, consideration.Curve, consideration.Weight);
                score *= factor;
            }

            return Saturate(score);
        }

        public static float ApplyCurve(float value, UtilityCurveType curve)
        {
            value = Saturate(value);

            return curve switch
            {
                UtilityCurveType.Linear => value,
                UtilityCurveType.Inverse => 1f - value,
                UtilityCurveType.Step => value >= 0.5f ? 1f : 0f,
                UtilityCurveType.Quadratic => value * value,
                UtilityCurveType.InverseQuadratic => 1f - value * value,
                _ => value
            };
        }

        public static float EvaluateConsiderationFactor(float value, UtilityCurveType curve, float weight)
        {
            var curved = ApplyCurve(value, curve);
            return Lerp(1f, curved, weight);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Saturate(t);
        }

        private static float Saturate(float value)
        {
            return MathF.Max(0f, MathF.Min(1f, value));
        }
    }
}
