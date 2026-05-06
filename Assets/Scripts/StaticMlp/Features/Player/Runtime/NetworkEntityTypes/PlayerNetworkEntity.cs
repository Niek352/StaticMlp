using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Player
{
    [NetworkEntityManifest(typeof(CharacterNetState))]
    public struct PlayerNetworkEntity : INetworkEntityType
    {
        public byte Id() => 1;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => PlayerGameplayFeature.PLAYER;
    }
}
