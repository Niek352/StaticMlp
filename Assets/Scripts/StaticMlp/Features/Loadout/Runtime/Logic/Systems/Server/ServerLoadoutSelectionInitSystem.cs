using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ServerLoadoutSelectionInitSystem : ISystem
    {
        public void Update()
        {
            foreach (var player in SW.Query<All<PlayerTag, NetworkIdentity>>().Entities())
            {
                if (player.Has<OwnerLoadoutSelection>())
                    continue;

                player.Set(LoadoutPreparationRules.DefaultSelection());
            }
        }
    }
}
