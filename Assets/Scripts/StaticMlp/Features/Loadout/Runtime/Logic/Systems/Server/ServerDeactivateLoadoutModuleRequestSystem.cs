using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ServerDeactivateLoadoutModuleRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, DeactivateLoadoutModuleRequestEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<DeactivateLoadoutModuleRequestEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
            {
                if (!ServerPeerPlayers.TryGetPlayer(evt.Value.SourcePeer, out var player))
                    continue;

                ref var loadout = ref player.Mut<ActiveModuleLoadout>();
                if (!CanDeactivate(evt.Value.SlotKind, evt.Value.SlotIndex))
                    continue;

                ActiveModuleLoadoutRules.Set(ref loadout, evt.Value.SlotKind, evt.Value.SlotIndex, default);
            }
        }

        private static bool CanDeactivate(EquipmentSlotKind slotKind, int slotIndex)
        {
            if (slotIndex < 0)
                return false;

            try
            {
                return slotIndex < SlotRuleCatalog.GetLimit(slotKind);
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
        }
    }
}
