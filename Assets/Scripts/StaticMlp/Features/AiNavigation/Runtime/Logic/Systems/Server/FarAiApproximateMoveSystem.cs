using FFS.Libraries.StaticEcs;
using StaticMlp.Game;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class FarAiApproximateMoveSystem : ISystem
    {
        public void Update()
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            var currentTime = (float)simulationTime.ElapsedSeconds;

            foreach (var entity in SW.Query<All<AiNavigationModeState, AiFarSimulationState>>().Entities())
                UpdateEntity(entity, currentTime, simulationTime.FixedStepSeconds);
        }

        private static void UpdateEntity(SW.Entity entity, float currentTime, float fixedStepSeconds)
        {
            ref var modeState = ref entity.Mut<AiNavigationModeState>();
            if (modeState.CurrentMode != AiNavigationMode.ApproximateMove)
                return;

            ref var farState = ref entity.Mut<AiFarSimulationState>();
            if (currentTime < farState.NextSimulationTime)
                return;

            var deltaTime = FarAiMovementRules.ResolveSimulationDelta(
                currentTime,
                fixedStepSeconds,
                farState.NextSimulationTime);
            farState.LogicalPosition = FarAiMovementRules.Advance(
                farState.LogicalPosition,
                farState.TargetPosition,
                deltaTime);
            farState.NextSimulationTime = FarAiMovementRules.ResolveNextSimulationTime(currentTime);

            if (!FarAiMovementRules.HasArrived(farState.LogicalPosition, farState.TargetPosition))
                return;

            modeState.CurrentMode = modeState.DesiredMode == AiNavigationMode.LocalNavMesh
                ? AiNavigationMode.WaitingForLocalNavMesh
                : modeState.DesiredMode;
        }
    }
}
