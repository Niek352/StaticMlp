using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ServerPreparedLoadoutSnapshotSystem : ISystem
    {
        public void Update()
        {
            foreach (var player in SW.Query<All<PlayerTag, NetworkIdentity, OwnerLoadoutSelection>>().Entities())
                player.Set(Stage1LoadoutRules.CreatePreparedSnapshot(player.Read<OwnerLoadoutSelection>()));
        }
    }
}
