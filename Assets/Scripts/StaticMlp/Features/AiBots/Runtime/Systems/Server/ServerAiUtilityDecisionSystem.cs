using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiUtilityDecisionSystem : ISystem
    {
        private const float DecisionInterval = 0.25f;

        public void Update()
        {
            if (!SW.HasResource<AiBehaviorCatalog>())
                throw new InvalidOperationException("AI behavior catalog resource is missing.");

            var catalog = SW.GetResource<AiBehaviorCatalog>();
            if (catalog == null)
                throw new InvalidOperationException("AI behavior catalog resource is null.");

            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBrain, AiBlackboard>>().Entities())
            {
                ref var brain = ref entity.Mut<AiBrain>();
                ref readonly var blackboard = ref entity.Read<AiBlackboard>();

                brain.DecisionCooldown -= Time.deltaTime;
                if (brain.DecisionCooldown > 0f)
                    continue;

                brain.DecisionCooldown = DecisionInterval;

                if (!catalog.TryGetBehavior(brain.BehaviorId, out var behavior))
                {
                    throw new InvalidOperationException(
                        $"AI behavior id {brain.BehaviorId} was not found in the runtime catalog.");
                }

                var selectedTask = AiUtilityEvaluator.SelectBestTask(in blackboard, in behavior);
                if (brain.CurrentTask == selectedTask)
                    continue;

                brain.CurrentTask = selectedTask;
                entity.Set(new AiTaskState
                {
                    Task = selectedTask,
                    Step = 0,
                    Timer = 0f
                });
            }
        }
    }
}
