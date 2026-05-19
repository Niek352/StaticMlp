using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class ServerActivateBuildModuleRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, ActivateBuildModuleRequestEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<ActivateBuildModuleRequestEvent>();
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

                if (!BuildModuleCatalog.TryGet(evt.Value.ModuleId, out var module))
                    continue;

                ref var loadout = ref player.Mut<ActiveBuildModuleLoadout>();
                if (!ActiveBuildModuleLoadoutRules.CanActivate(loadout, module, evt.Value.SlotKind, evt.Value.SlotIndex))
                    continue;

                ActiveBuildModuleLoadoutRules.Set(ref loadout, evt.Value.SlotKind, evt.Value.SlotIndex, module.Id);
            }
        }
    }
}
