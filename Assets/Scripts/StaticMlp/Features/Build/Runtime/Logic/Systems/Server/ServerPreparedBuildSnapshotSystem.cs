using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class ServerPreparedBuildSnapshotSystem : ISystem
    {
        public void Update()
        {
            foreach (var player in SW.Query<All<PlayerTag, NetworkIdentity, OwnerBuildSelection>>().Entities())
                player.Set(Stage1BuildRules.CreatePreparedSnapshot(player.Read<OwnerBuildSelection>()));
        }
    }
}
