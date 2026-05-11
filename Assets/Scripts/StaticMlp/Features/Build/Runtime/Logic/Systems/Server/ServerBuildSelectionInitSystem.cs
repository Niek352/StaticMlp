using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class ServerBuildSelectionInitSystem : ISystem
    {
        public void Update()
        {
            foreach (var player in SW.Query<All<PlayerTag, NetworkIdentity>>().Entities())
            {
                if (player.Has<OwnerBuildSelection>())
                    continue;

                player.Set(Stage1BuildRules.DefaultSelection());
            }
        }
    }
}
