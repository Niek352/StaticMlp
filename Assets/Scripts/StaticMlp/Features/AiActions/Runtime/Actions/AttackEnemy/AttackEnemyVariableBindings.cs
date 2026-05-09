using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiActions
{
    public static class AttackEnemyVariableBindings
    {
        public const ushort Health01 = 1201;
        public const ushort Fear = 1202;
        public const ushort EnemyDistance01 = 1203;
        public const ushort HasEnemy = 1204;

        public static readonly AiBlackboardFloatBinding[] Bindings =
        {
            new(Health01, ReadHealth01),
            new(Fear, ReadFear),
            new(EnemyDistance01, ReadEnemyDistance01),
            new(HasEnemy, ReadHasEnemy)
        };

        public static readonly AiBehaviorTaskContribution[] UtilityTaskContributions =
        {
            new()
            {
                BehaviorId = AiBehaviorIds.Default,
                Considerations = new[]
                {
                    new UtilityConsideration
                    {
                        VariableId = Health01,
                        Curve = UtilityCurveType.Linear,
                        Weight = 0.8f
                    },
                    new UtilityConsideration
                    {
                        VariableId = Fear,
                        Curve = UtilityCurveType.Inverse,
                        Weight = 0.7f
                    },
                    new UtilityConsideration
                    {
                        VariableId = EnemyDistance01,
                        Curve = UtilityCurveType.Inverse,
                        Weight = 0.8f
                    },
                    new UtilityConsideration
                    {
                        VariableId = HasEnemy,
                        Curve = UtilityCurveType.Linear,
                        Weight = 1f
                    }
                }
            }
        };

        private static float ReadHealth01(SW.Entity entity) => AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Health01);

        private static float ReadFear(SW.Entity entity) => AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Fear);

        private static float ReadEnemyDistance01(SW.Entity entity)
        {
            return Mathf.Clamp01(AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.EnemyDistance, 999f) / 25f);
        }

        private static float ReadHasEnemy(SW.Entity entity)
        {
            return AiBlackboardAccess.TryGetEntity(entity, AiCoreVariableIds.Enemy, out _) ? 1f : 0f;
        }
    }
}
