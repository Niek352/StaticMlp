using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Features.Builtin;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.NetworkEntityTypes {
    [NetworkEntityManifest(typeof(CharacterNetState))]
    public struct PlayerNetworkEntity : INetworkEntityType {
        public byte Id() => 1;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => BuiltinGameplayFeature.PLAYER;
    }
}
