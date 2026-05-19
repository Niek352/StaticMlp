using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ServerActiveModuleLoadoutInitSystem : ISystem
    {
        public void Update()
        {
            foreach (var player in SW.Query<All<PlayerTag, NetworkIdentity>>().Entities())
            {
                if (player.Has<ActiveModuleLoadout>())
                    continue;

                player.Set(new ActiveModuleLoadout());
            }
        }
    }
}
