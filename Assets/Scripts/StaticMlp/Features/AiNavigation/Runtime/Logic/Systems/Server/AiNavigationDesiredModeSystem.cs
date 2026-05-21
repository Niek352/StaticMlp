using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationDesiredModeSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<AiNavigationModeState, AiFarSimulationState, LocalNavAttachState>>().Entities())
                UpdateDesiredMode(entity);
        }

        private static void UpdateDesiredMode(SW.Entity entity)
        {
            ref var modeState = ref entity.Mut<AiNavigationModeState>();
            ref readonly var attachState = ref entity.Read<LocalNavAttachState>();

            var desired = attachState.IsReady
                ? AiNavigationMode.LocalNavMesh
                : AiNavigationMode.ApproximateMove;

            modeState.DesiredMode = desired;
        }
    }
}
