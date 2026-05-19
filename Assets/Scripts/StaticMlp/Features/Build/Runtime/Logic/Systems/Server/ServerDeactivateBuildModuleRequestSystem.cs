using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class ServerDeactivateBuildModuleRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, DeactivateBuildModuleRequestEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<DeactivateBuildModuleRequestEvent>();
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

                ref var loadout = ref player.Mut<ActiveBuildModuleLoadout>();
                if (!CanDeactivate(evt.Value.SlotKind, evt.Value.SlotIndex))
                    continue;

                ActiveBuildModuleLoadoutRules.Set(ref loadout, evt.Value.SlotKind, evt.Value.SlotIndex, default);
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
