using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class AiNavigationLogicFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            // Keep Phase 01 free of runtime logic. The contracts become available through
            // MultiplayerWorldBootstrap.RegisterAll(...) once this logic assembly is loaded
            // and references StaticMlp.Features.AiNavigation.Contracts.
            //
            // Phase 02 systems belong after combat-cell/threat preparation and before
            // director phase/source scoring, without editing MultiplayerSystemBootstrap.
        }
    }
}
