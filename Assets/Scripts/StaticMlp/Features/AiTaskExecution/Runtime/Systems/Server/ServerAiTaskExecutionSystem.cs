using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using System;

namespace StaticMlp.Features.AiTaskExecution
{
    public sealed class ServerAiTaskExecutionSystem : ISystem
    {
        public void Update()
        {
            if (!SW.HasResource<AiActionCatalog>())
                throw new InvalidOperationException("AI action catalog resource is missing.");

            var catalog = SW.GetResource<AiActionCatalog>();
            if (catalog == null)
                throw new InvalidOperationException("AI action catalog resource is null.");

            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBrain, AiTaskState, SW.Multi<AiBlackboardEntry>, CharacterNetState>>().Entities())
            {
                ref var task = ref entity.Mut<AiTaskState>();

                var executor = EnsureActiveExecutor(entity, ref task, catalog);
                executor.Execute(entity, ref task);
                EnsurePostExecuteTransition(entity, ref task, catalog);
            }
        }

        private static IAiTaskExecutor EnsureActiveExecutor(SW.Entity entity, ref AiTaskState task, AiActionCatalog catalog)
        {
            var desiredExecutor = catalog.ResolveExecutor(task.Task);
            if (!task.HasActiveTask || task.ActiveTask != desiredExecutor.TaskType)
                TransitionToExecutor(entity, ref task, catalog, desiredExecutor);

            return catalog.ResolveExecutor(task.ActiveTask);
        }

        private static void EnsurePostExecuteTransition(SW.Entity entity, ref AiTaskState task, AiActionCatalog catalog)
        {
            var desiredExecutor = catalog.ResolveExecutor(task.Task);
            if (task.HasActiveTask && task.ActiveTask == desiredExecutor.TaskType)
                return;

            TransitionToExecutor(entity, ref task, catalog, desiredExecutor);
        }

        private static void TransitionToExecutor(
            SW.Entity entity,
            ref AiTaskState task,
            AiActionCatalog catalog,
            IAiTaskExecutor nextExecutor)
        {
            if (task.HasActiveTask)
            {
                var currentExecutor = catalog.ResolveExecutor(task.ActiveTask);
                currentExecutor.Exit(entity, ref task);
            }

            task.ActiveTask = nextExecutor.TaskType;
            task.HasActiveTask = true;
            nextExecutor.Enter(entity, ref task);
        }
    }
}
