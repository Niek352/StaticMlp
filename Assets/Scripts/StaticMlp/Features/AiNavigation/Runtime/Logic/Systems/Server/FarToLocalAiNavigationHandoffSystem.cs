using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class FarToLocalAiNavigationHandoffSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<AiNavigationModeState, AiFarSimulationState, LocalNavAttachState>>().Entities())
                TryHandoff(entity);
        }

        private static void TryHandoff(SW.Entity entity)
        {
            ref var modeState = ref entity.Mut<AiNavigationModeState>();
            if (!FarAiMovementRules.RequiresLocalNavigationHandoff(in modeState))
                return;

            ref readonly var attachState = ref entity.Read<LocalNavAttachState>();
            if (!attachState.IsReady)
                return;

            ref readonly var farState = ref entity.Read<AiFarSimulationState>();
            SW.SendEvent(new LocalNavigationHandoffRequestEvent(
                entity.GID,
                farState.LogicalPosition,
                farState.TargetPosition,
                FarAiMovementRules.ResolveStopDistance()));
            modeState.CurrentMode = AiNavigationMode.LocalNavMesh;
        }
    }
}
