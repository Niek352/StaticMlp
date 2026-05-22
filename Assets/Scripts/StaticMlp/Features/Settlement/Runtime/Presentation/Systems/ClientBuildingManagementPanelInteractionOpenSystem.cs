using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Interaction;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientBuildingManagementPanelInteractionOpenSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, InteractPressedEvent> _interactEvents;

        public void Init()
        {
            _interactEvents = CW.RegisterEventReceiver<InteractPressedEvent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _interactEvents);
        }

        public void Update()
        {
            foreach (var evt in _interactEvents)
            {
                ref readonly var press = ref evt.Value;
                if (press.Kind != InteractableKind.ConstructionSite
                    && press.Kind != InteractableKind.FinishedBuilding)
                {
                    continue;
                }

                if (!press.Target.TryUnpack<ClientCoreWT>(out var target))
                    throw new InvalidOperationException($"Interact target {press.Target.Raw} is not a client entity.");

                if (!target.Has<ConstructionSiteState>())
                    throw new InvalidOperationException($"Interact target {press.Target.Raw} is not a construction/building entity.");

                ref var session = ref CW.GetResource<BuildingPanelSession>();
                session.Open(BuildingPanelRoute.Resolve(target), openedFromInteraction: true);
            }
        }
    }
}
