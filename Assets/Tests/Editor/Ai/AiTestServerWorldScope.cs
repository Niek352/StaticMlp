using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiActions;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiTestServerWorldScope : IDisposable
    {
        private const float DEFAULT_FIXED_STEP_SECONDS = 1f / 30f;

        public AiTestServerWorldScope()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(ServerWT).Assembly,
                typeof(AiActionCatalog).Assembly,
                typeof(AiActionsGameplayFeature).Assembly,
                typeof(ConstructionRules).Assembly,
                typeof(SettlementWorkersGameplayFeature).Assembly,
                typeof(Health).Assembly,
                typeof(CharacterNetState).Assembly);
            SW.Initialize();

            Catalog = AiActionCatalog.Discover(new AiTaskExecutionTransitions());
            SW.SetResource(new SimulationTime
            {
                FixedStepSeconds = DEFAULT_FIXED_STEP_SECONDS,
            });
            SW.SetResource(Catalog);
            CreateSettlementSharedResources();
        }

        public AiActionCatalog Catalog { get; }
        public SimulationTime SimulationTime => SW.GetResource<SimulationTime>();

        public SW.Entity CreateSettlementSharedResources(int wood = 50, int stone = 25)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<SettlementResourceStorageTag>();
            entity.Set(new SettlementSharedResources
            {
                Wood = wood,
                Stone = stone
            });
            return entity;
        }

        public SW.Entity CreateBot(Vector3 position, ushort behaviorId = CombatEnemyBehaviorIds.Default)
        {
            const float maxHealth = 100f;
            var entity = SW.NewEntity<Default>();
            entity.Set<ServerOwned>();
            entity.Set<AiAgentTag>();
            entity.Set(new Health
            {
                Current = maxHealth,
                Max = maxHealth
            });
            entity.Set(new AiBrain
            {
                BehaviorId = behaviorId,
                CurrentTask = AiTaskType.Idle,
                NextDecisionTick = 0
            });
            entity.Set(new AiTaskState
            {
                Task = AiTaskType.Idle,
                ActiveTask = AiTaskType.Idle,
                HasActiveTask = false,
                Step = 0,
                ElapsedTicks = 0
            });
            entity.Set(new CharacterNetState
            {
                Position = position,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity
            });
            entity.Add<SW.Multi<AiBlackboardEntry>>();
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Hunger, 0.1f);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Health01, 1f);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Fear, 0.05f);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.EnemyDistance, 999f);
            AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.WoodStorage01, 1f);
            AiBlackboardAccess.SetVector(entity, AiCoreVariableIds.LastKnownEnemyPosition, position);
            return entity;
        }

        public SW.Entity CreateSettlementAnchor(
            SettlementAnchorId anchorId,
            Stage1SettlementProgressStage stage = Stage1SettlementProgressStage.CampRepaired)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new Stage1SettlementProgression
            {
                AnchorId = anchorId.Value,
                Stage = stage
            });
            return entity;
        }

        public SW.Entity CreateWorker(
            SettlementAnchorId anchorId,
            Vector3 position,
            SettlementWorkerAssignmentStatus status = SettlementWorkerAssignmentStatus.Unassigned)
        {
            var entity = CreateBot(position, SettlementWorkerBehaviorIds.PeacefulBuilder);
            entity.Set<SettlementWorkerTag>();
            entity.Set(new SettlementWorkerIdentity
            {
                HomeAnchorId = anchorId.Value,
                RoleId = WorkerRoleCatalog.CampBuilderId.Value
            });
            entity.Set(new SettlementWorkerAssignment
            {
                Status = status,
                AnchorId = status == SettlementWorkerAssignmentStatus.Assigned ? anchorId.Value : (ushort)0
            });
            return entity;
        }

        public SW.Entity CreateConstructionSite(Vector3 position, ConstructionPhase phase, bool resourcesComplete)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<ConstructionSiteTag>();
            entity.Set(new ConstructionSiteState
            {
                BuildingId = 1,
                Phase = phase
            });
            entity.Set(new ConstructionResources
            {
                WoodRequired = 10,
                StoneRequired = 5,
                WoodDelivered = resourcesComplete ? 10 : 0,
                StoneDelivered = resourcesComplete ? 5 : 0
            });
            entity.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            entity.Set(new ConstructionProgress
            {
                BuildWorkRequired = 20f,
                BuildWorkDone = 0f
            });
            return entity;
        }

        public void Dispose()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }
    }
}
