using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ServerActivateLoadoutModuleRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, ActivateLoadoutModuleRequestEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<ActivateLoadoutModuleRequestEvent>();
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

                if (!LoadoutModuleCatalog.TryGet(evt.Value.ModuleId, out var module))
                    continue;

                ref var loadout = ref player.Mut<ActiveModuleLoadout>();
                if (!ActiveModuleLoadoutRules.CanActivate(loadout, module, evt.Value.SlotKind, evt.Value.SlotIndex))
                    continue;

                ActiveModuleLoadoutRules.Set(ref loadout, evt.Value.SlotKind, evt.Value.SlotIndex, module.Id);
            }
        }
    }
}
