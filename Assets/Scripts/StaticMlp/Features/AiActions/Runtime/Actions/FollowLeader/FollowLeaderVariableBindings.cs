using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public static class FollowLeaderVariableBindings
    {
        public const ushort HasLeader = 1301;
        public const ushort Fear = 1302;
        public const ushort Hunger = 1303;

        public static readonly AiBlackboardFloatBinding[] Bindings =
        {
            new(HasLeader, ReadHasLeader),
            new(Fear, ReadFear),
            new(Hunger, ReadHunger)
        };

        public static readonly AiBehaviorTaskContribution[] UtilityTaskContributions =
        {
            new()
            {
                BehaviorId = AiBehaviorIds.Default,
                Considerations = CreateCommonConsiderations()
            },
            new()
            {
                BehaviorId = AiBehaviorIds.PeacefulBuilder,
                Considerations = CreateCommonConsiderations()
            }
        };

        private static UtilityConsideration[] CreateCommonConsiderations()
        {
            return new[]
            {
                new UtilityConsideration
                {
                    VariableId = HasLeader,
                    Curve = UtilityCurveType.Linear,
                    Weight = 1f
                },
                new UtilityConsideration
                {
                    VariableId = Fear,
                    Curve = UtilityCurveType.Inverse,
                    Weight = 0.3f
                },
                new UtilityConsideration
                {
                    VariableId = Hunger,
                    Curve = UtilityCurveType.Inverse,
                    Weight = 0.2f
                }
            };
        }

        private static float ReadHasLeader(SW.Entity entity)
        {
            return AiBlackboardAccess.TryGetEntity(entity, AiCoreVariableIds.Leader, out _) ? 1f : 0f;
        }

        private static float ReadFear(SW.Entity entity) => AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Fear);

        private static float ReadHunger(SW.Entity entity) => AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Hunger);
    }
}
