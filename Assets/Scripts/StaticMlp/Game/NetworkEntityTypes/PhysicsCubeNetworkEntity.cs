using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Features.Builtin;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.NetworkEntityTypes {
    [NetworkEntityManifest(typeof(PhysicsCubeNetState))]
    public struct PhysicsCubeNetworkEntity : INetworkEntityType {
        public byte Id() => 2;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => BuiltinGameplayFeature.PHYSICS_CUBE;
    }
}
