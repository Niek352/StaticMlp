using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Interaction;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementInteractionPromptStateSystem : ISystem
    {
        private const string INTERACT_INPUT_HINT = "E";

        public void Update()
        {
            ref var prompt = ref CW.GetResource<InteractionPromptState>();
            prompt.Hide();

            ref readonly var panelSession = ref CW.GetResource<BuildingPanelSession>();
            if (panelSession.IsOpen)
                return;

            ref readonly var focus = ref CW.GetResource<InteractionFocus>();
            if (!focus.HasFocus)
                return;

            if (focus.Kind != InteractableKind.ConstructionSite
                && focus.Kind != InteractableKind.FinishedBuilding)
            {
                return;
            }

            var (label, effect) = ResolveBuildingPrompt(focus.Target);
            prompt.Show(focus.Target, focus.Kind, INTERACT_INPUT_HINT, label, effect);
        }

        private static (string label, string effect) ResolveBuildingPrompt(EntityGID target)
        {
            if (!target.TryUnpack<ClientCoreWT>(out var site))
                throw new InvalidOperationException($"Interaction prompt target {target.Raw} is not a client entity.");

            if (!site.Has<ConstructionSiteState>())
                throw new InvalidOperationException($"Interaction prompt target {target.Raw} is not a construction/building entity.");

            ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(site);
            var definition = BuildingCatalogData.Get(new BuildingId(state.BuildingId));

            if (state.Phase == ConstructionPhase.Completed)
                return ($"Open {definition.DisplayName}", "Opens the building management panel.");

            if (ConstructionResourcesAccess.IsProjectedComplete(site))
                return ("Manage Construction", "Opens construction work and details.");

            return ("Open Construction", "Opens construction resources and actions.");
        }
    }
}
