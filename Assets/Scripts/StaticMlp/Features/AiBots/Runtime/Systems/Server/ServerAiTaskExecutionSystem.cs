using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiTaskExecutionSystem : ISystem
    {
        private const float BuildInteractionRange = 4f;
        private const float BuildWorkPerSecond = 8f;

        public void Update()
        {
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBrain, AiTaskState, AiBlackboard, CharacterNetState>>().Entities())
            {
                ref var task = ref entity.Mut<AiTaskState>();
                ref var blackboard = ref entity.Mut<AiBlackboard>();

                switch (task.Task)
                {
                    case AiTaskType.Flee:
                        ExecuteFlee(entity, ref task, ref blackboard);
                        break;

                    case AiTaskType.AttackEnemy:
                        ExecuteAttackEnemy(entity, ref task, ref blackboard);
                        break;

                    case AiTaskType.FollowLeader:
                        ExecuteFollowLeader(entity, ref task, ref blackboard);
                        break;

                    case AiTaskType.BuildConstruction:
                        ExecuteBuildConstruction(entity, ref task, ref blackboard);
                        break;

                    default:
                        ExecuteIdle(entity, ref task);
                        break;
                }
            }
        }

        private static void ExecuteFlee(SW.Entity entity, ref AiTaskState task, ref AiBlackboard blackboard)
        {
            ClearAttackRequest(entity);

            ref readonly var state = ref entity.Read<CharacterNetState>();
            var away = state.Position - blackboard.LastKnownEnemyPosition;
            if (away.sqrMagnitude < 0.001f)
                away = Vector3.forward;

            away.Normalize();
            entity.Set(new AiMoveRequest
            {
                Destination = state.Position + away * 8f,
                StopDistance = 0.5f
            });
            task.Timer += Time.deltaTime;
        }

        private static void ExecuteAttackEnemy(SW.Entity entity, ref AiTaskState task, ref AiBlackboard blackboard)
        {
            if (!blackboard.Enemy.TryUnpack<ServerWT>(out var enemy) || !enemy.Has<CharacterNetState>())
            {
                ResetToIdle(entity, ref task);
                return;
            }

            ref readonly var enemyState = ref enemy.Read<CharacterNetState>();
            blackboard.LastKnownEnemyPosition = enemyState.Position;
            entity.Set(new AiMoveRequest
            {
                Destination = enemyState.Position,
                StopDistance = 1.8f
            });
            entity.Set(new AiAttackRequest
            {
                Target = blackboard.Enemy
            });
            task.Timer += Time.deltaTime;
        }

        private static void ExecuteFollowLeader(SW.Entity entity, ref AiTaskState task, ref AiBlackboard blackboard)
        {
            ClearAttackRequest(entity);

            if (!blackboard.Leader.TryUnpack<ServerWT>(out var leader) || !leader.Has<CharacterNetState>())
            {
                ResetToIdle(entity, ref task);
                return;
            }

            ref readonly var leaderState = ref leader.Read<CharacterNetState>();
            entity.Set(new AiMoveRequest
            {
                Destination = leaderState.Position,
                StopDistance = 3f
            });
            task.Timer += Time.deltaTime;
        }

        private static void ExecuteBuildConstruction(SW.Entity entity, ref AiTaskState task, ref AiBlackboard blackboard)
        {
            ClearAttackRequest(entity);

            if (!ConstructionSiteQuery.TryGetBuildableSite(blackboard.WorkTarget, out var site))
            {
                ResetToIdle(entity, ref task);
                return;
            }

            ref readonly var siteState = ref site.Read<ConstructionSiteState>();
            ref readonly var siteResources = ref site.Read<ConstructionResources>();
            if (!ConstructionRules.CanBuild(in siteState, in siteResources))
            {
                ResetToIdle(entity, ref task);
                return;
            }

            ref readonly var siteTransform = ref site.Read<ConstructionTransform>();
            ref readonly var characterState = ref entity.Read<CharacterNetState>();
            var toSite = siteTransform.Position - characterState.Position;
            if (toSite.sqrMagnitude > BuildInteractionRange * BuildInteractionRange)
            {
                entity.Set(new AiMoveRequest
                {
                    Destination = siteTransform.Position,
                    StopDistance = BuildInteractionRange - 0.5f
                });
                task.Timer += Time.deltaTime;
                return;
            }

            StopMovement(entity);

            ref var mutableSiteState = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var mutableProgress = ref ReplicationMut.Mut<ConstructionProgress>(site);
            if (!ConstructionRules.ApplyBuildWork(
                    ref mutableSiteState,
                    ref mutableProgress,
                    in siteResources,
                    BuildWorkPerSecond * Time.deltaTime,
                    BuildWorkPerSecond))
            {
                ResetToIdle(entity, ref task);
                return;
            }

            task.Timer += Time.deltaTime;
            if (mutableProgress.IsComplete)
                ResetToIdle(entity, ref task);
        }

        private static void ExecuteIdle(SW.Entity entity, ref AiTaskState task)
        {
            task.Timer += Time.deltaTime;
            ClearAttackRequest(entity);
            StopMovement(entity);
        }

        private static void ResetToIdle(SW.Entity entity, ref AiTaskState task)
        {
            ref var brain = ref entity.Mut<AiBrain>();
            brain.CurrentTask = AiTaskType.Idle;
            task.Task = AiTaskType.Idle;
            task.Step = 0;
            task.Timer = 0f;
            ClearAttackRequest(entity);
            StopMovement(entity);
        }

        private static void ClearAttackRequest(SW.Entity entity)
        {
            if (entity.Has<AiAttackRequest>())
                entity.Delete<AiAttackRequest>();
        }

        private static void StopMovement(SW.Entity entity)
        {
            if (entity.Has<AiMoveRequest>())
                entity.Delete<AiMoveRequest>();

            ref readonly var state = ref entity.Read<CharacterNetState>();
            if (state.Velocity == Vector3.zero)
                return;

            ref var replicatedState = ref ReplicationMut.Mut<CharacterNetState>(entity);
            replicatedState.Velocity = Vector3.zero;
        }
    }
}
