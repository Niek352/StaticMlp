using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiActions;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiTestServerWorldScope : IDisposable
    {
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
                typeof(Health).Assembly,
                typeof(CharacterNetState).Assembly);
            SW.Initialize();

            Catalog = AiActionCatalog.Discover(new AiTaskExecutionTransitions());
            SW.SetResource(Catalog);
        }

        public AiActionCatalog Catalog { get; }

        public SW.Entity CreateBot(Vector3 position, ushort behaviorId = AiBehaviorIds.Monster)
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
                DecisionCooldown = 0f
            });
            entity.Set(new AiTaskState
            {
                Task = AiTaskType.Idle,
                ActiveTask = AiTaskType.Idle,
                HasActiveTask = false,
                Step = 0,
                Timer = 0f
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
