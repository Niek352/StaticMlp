using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public static class FarAiMovementRules
    {
        private const float APPROXIMATE_MOVE_SPEED = 3.5f;
        private const float SIMULATION_INTERVAL = 0.25f;
        private const float DEFAULT_STOP_DISTANCE = 0.5f;

        public static bool IsFarSimulationMode(AiNavigationMode mode)
        {
            return mode == AiNavigationMode.ApproximateMove
                   || mode == AiNavigationMode.WaitingForLocalNavMesh;
        }

        public static bool RequiresLocalNavigationHandoff(in AiNavigationModeState modeState)
        {
            return modeState.DesiredMode == AiNavigationMode.LocalNavMesh
                   && IsFarSimulationMode(modeState.CurrentMode);
        }

        public static float ResolveStopDistance()
        {
            return DEFAULT_STOP_DISTANCE;
        }

        public static float ResolveNextSimulationTime(float currentTime)
        {
            return currentTime + SIMULATION_INTERVAL;
        }

        public static float ResolveSimulationDelta(
            float currentTime,
            float fixedStepSeconds,
            float nextSimulationTime)
        {
            if (fixedStepSeconds <= 0f)
                return SIMULATION_INTERVAL;

            if (nextSimulationTime <= 0f)
                return fixedStepSeconds;

            var previousSimulationTime = nextSimulationTime - SIMULATION_INTERVAL;
            return math.max(fixedStepSeconds, currentTime - previousSimulationTime);
        }

        public static bool HasArrived(float3 logicalPosition, float3 targetPosition)
        {
            var stopDistance = ResolveStopDistance();
            return math.distancesq(logicalPosition, targetPosition) <= stopDistance * stopDistance;
        }

        public static float3 Advance(float3 logicalPosition, float3 targetPosition, float deltaTime)
        {
            var delta = targetPosition - logicalPosition;
            var distanceSq = math.lengthsq(delta);
            if (distanceSq <= 0f)
                return logicalPosition;

            var distance = math.sqrt(distanceSq);
            var maxStep = APPROXIMATE_MOVE_SPEED * math.max(deltaTime, 0f);
            var stopDistance = ResolveStopDistance();
            if (distance <= math.max(maxStep, stopDistance))
                return targetPosition;

            return logicalPosition + delta / distance * maxStep;
        }
    }
}
