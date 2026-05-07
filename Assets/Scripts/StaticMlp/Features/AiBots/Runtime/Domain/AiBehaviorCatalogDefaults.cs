namespace StaticMlp.Features.AiBots
{
    public static class AiBehaviorCatalogDefaults
    {
        public const ushort DefaultBehaviorId = 1;
        public const ushort PeacefulBuilderBehaviorId = 2;

        public static AiBehaviorCatalog Create()
        {
            return new AiBehaviorCatalog(new[]
            {
                new AiBehaviorDefinition
                {
                    BehaviorId = DefaultBehaviorId,
                    Tasks = new[]
                    {
                        new UtilityTaskDefinition
                        {
                            Task = AiTaskType.Flee,
                            Considerations = new[]
                            {
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Health01,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.8f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Fear,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 0.8f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.EnemyDistance01,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.6f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.HasEnemy,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 1f
                                }
                            }
                        },
                        new UtilityTaskDefinition
                        {
                            Task = AiTaskType.AttackEnemy,
                            Considerations = new[]
                            {
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Health01,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 0.8f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Fear,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.7f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.EnemyDistance01,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.8f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.HasEnemy,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 1f
                                }
                            }
                        },
                        new UtilityTaskDefinition
                        {
                            Task = AiTaskType.FollowLeader,
                            Considerations = new[]
                            {
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.HasLeader,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 1f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Fear,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.3f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Hunger,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.2f
                                }
                            }
                        }
                    }
                },
                new AiBehaviorDefinition
                {
                    BehaviorId = PeacefulBuilderBehaviorId,
                    Tasks = new[]
                    {
                        new UtilityTaskDefinition
                        {
                            Task = AiTaskType.Flee,
                            Considerations = new[]
                            {
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Health01,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.85f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Fear,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 0.85f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.EnemyDistance01,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.6f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.HasEnemy,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 1f
                                }
                            }
                        },
                        new UtilityTaskDefinition
                        {
                            Task = AiTaskType.BuildConstruction,
                            Considerations = new[]
                            {
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.HasWorkTarget,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 1f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Health01,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 0.4f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Fear,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.4f
                                }
                            }
                        },
                        new UtilityTaskDefinition
                        {
                            Task = AiTaskType.FollowLeader,
                            Considerations = new[]
                            {
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.HasLeader,
                                    Curve = UtilityCurveType.Linear,
                                    Weight = 1f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Fear,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.3f
                                },
                                new UtilityConsideration
                                {
                                    Key = AiBlackboardKey.Hunger,
                                    Curve = UtilityCurveType.Inverse,
                                    Weight = 0.2f
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
