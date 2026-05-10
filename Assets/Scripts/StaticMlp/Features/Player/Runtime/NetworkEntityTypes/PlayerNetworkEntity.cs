using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Player
{
    [NetworkEntityManifest(typeof(CharacterNetState), typeof(Health))]
    public struct PlayerNetworkEntity : INetworkEntityType
    {
        public byte Id() => 1;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => PlayerGameplayFeature.PLAYER;
    }
}
