using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public static class AiUtilityEvaluator
    {
        public static AiTaskType SelectBestTask(in AiBlackboard blackboard, in AiBehaviorDefinition behavior)
        {
            var bestScore = 0f;
            var bestTask = AiTaskType.Idle;

            for (var i = 0; i < behavior.Tasks.Length; i++)
            {
                ref readonly var task = ref behavior.Tasks[i];
                var score = EvaluateTask(in blackboard, task.Considerations);
                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestTask = task.Task;
            }

            return bestTask;
        }

        public static float EvaluateTask(in AiBlackboard blackboard, UtilityConsideration[] considerations)
        {
            if (considerations == null || considerations.Length == 0)
                return 0f;

            var score = 1f;
            for (var i = 0; i < considerations.Length; i++)
            {
                var consideration = considerations[i];
                var value = ReadBlackboardValue(in blackboard, consideration.Key);
                var curved = ApplyCurve(value, consideration.Curve);
                score *= Lerp(1f, curved, consideration.Weight);
            }

            return Saturate(score);
        }

        private static float ReadBlackboardValue(in AiBlackboard blackboard, AiBlackboardKey key)
        {
            return key switch
            {
                AiBlackboardKey.Hunger => blackboard.Hunger,
                AiBlackboardKey.Health01 => blackboard.Health01,
                AiBlackboardKey.Fear => blackboard.Fear,
                AiBlackboardKey.EnemyDistance01 => Saturate(blackboard.EnemyDistance / 25f),
                AiBlackboardKey.WoodStorage01 => blackboard.WoodStorage01,
                AiBlackboardKey.HasEnemy => blackboard.Enemy.TryUnpack<ServerWT>(out _) ? 1f : 0f,
                AiBlackboardKey.HasLeader => blackboard.Leader.TryUnpack<ServerWT>(out _) ? 1f : 0f,
                AiBlackboardKey.HasWorkTarget => blackboard.WorkTarget.TryUnpack<ServerWT>(out _) ? 1f : 0f,
                _ => 0f
            };
        }

        private static float ApplyCurve(float value, UtilityCurveType curve)
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
