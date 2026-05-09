using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public static class BuildConstructionVariableBindings
    {
        public const ushort HAS_BUILD_TARGET = 1401;
        public const ushort HEALTH01 = 1402;
        public const ushort FEAR = 1403;

        public static readonly AiBlackboardFloatBinding[] Bindings =
        {
            new(HAS_BUILD_TARGET, ReadHasBuildTarget),
            new(HEALTH01, ReadHealth01),
            new(FEAR, ReadFear)
        };

        public static readonly AiBehaviorTaskContribution[] UtilityTaskContributions =
        {
            new()
            {
                BehaviorId = AiBehaviorIds.PeacefulBuilder,
                Considerations = new[]
                {
                    new UtilityConsideration
                    {
                        VariableId = HAS_BUILD_TARGET,
                        Curve = UtilityCurveType.Linear,
                        Weight = 1f
                    },
                    new UtilityConsideration
                    {
                        VariableId = HEALTH01,
                        Curve = UtilityCurveType.Linear,
                        Weight = 0.4f
                    },
                    new UtilityConsideration
                    {
                        VariableId = FEAR,
                        Curve = UtilityCurveType.Inverse,
                        Weight = 0.4f
                    }
                }
            }
        };

        private static float ReadHasBuildTarget(SW.Entity entity)
        {
            return AiBlackboardAccess.TryGetEntity(entity, BuildConstructionCollectVariables.BuildTargetSite, out _) ? 1f : 0f;
        }

        private static float ReadHealth01(SW.Entity entity) => AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Health01);

        private static float ReadFear(SW.Entity entity) => AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Fear);
    }
}
