using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiUtilityDecisionSystem : ISystem
    {
        private const float DECISION_INTERVAL = 0.25f;

        public void Update()
        {
            var catalog = SW.GetResource<AiActionCatalog>();
            var simulationTime = SW.GetResource<SimulationTime>();
            var decisionIntervalTicks = simulationTime.SecondsToTicks(DECISION_INTERVAL);

            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBrain, AiTaskState, SW.Multi<AiBlackboardEntry>>>().Entities())
            {
                ref var brain = ref entity.Mut<AiBrain>();

                if (simulationTime.ServerTick < brain.NextDecisionTick)
                    continue;

                brain.NextDecisionTick = simulationTime.ServerTick + decisionIntervalTicks;

                if (!catalog.TryGetBehavior(brain.BehaviorId, out var behavior))
                {
                    throw new InvalidOperationException(
                        $"AI behavior id {brain.BehaviorId} was not found in the runtime catalog.");
                }

                var selectedTask = AiUtilityEvaluator.SelectBestTask(entity, catalog, in behavior);
                if (brain.CurrentTask == selectedTask)
                    continue;

                brain.CurrentTask = selectedTask;
                ref var task = ref entity.Mut<AiTaskState>();
                task.Task = selectedTask;
                task.Step = 0;
                task.ElapsedTicks = 0;
            }
        }
    }
}
