using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class GlobalRouteRequestSystem : ISystem
    {
        private const float DEFAULT_REPATH_INTERVAL = 2f;

        public void Update()
        {
            foreach (var entity in SW.Query<All<AiNavigationModeState, AiFarSimulationState>>().Entities())
                TryRequestRoute(entity);
        }

        private static void TryRequestRoute(SW.Entity entity)
        {
            ref readonly var modeState = ref entity.Read<AiNavigationModeState>();
            if (!FarAiMovementRules.IsFarSimulationMode(modeState.CurrentMode))
                return;

            ref readonly var farState = ref entity.Read<AiFarSimulationState>();

            if (entity.Has<GlobalRoute>())
            {
                ref var route = ref entity.Mut<GlobalRoute>();
                if (!math.all(route.FinalTargetPosition == farState.TargetPosition))
                {
                    route.FinalTargetPosition = farState.TargetPosition;
                    route.RepathTimer = 0f;
                }

                return;
            }

            if (!math.all(math.isfinite(farState.TargetPosition)))
                return;

            var routeComponent = new GlobalRoute
            {
                FinalTargetPosition = farState.TargetPosition,
                RepathTimer = 0f,
                RepathInterval = DEFAULT_REPATH_INTERVAL
            };

            entity.Set(routeComponent);
        }
    }
}
