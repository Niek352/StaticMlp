using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Features.Builtin;
using StaticMlp.Game.NetworkEntityTypes;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Game {
    public sealed class PhysicsCubeFactory : NetEntityFactory<PhysicsCubeNetworkEntity>, IResource {
        public EntityGID ServerSpawnPhysicsCube(NetworkPeerId owner, Vector3 spawnPosition, Quaternion rotation)
        {
            var ent = CreateEntity(owner, NetworkAuthority.Server, BuiltinGameplayFeature.PHYSICS_CUBE);
            ent.Set<CubeTag>();
            ent.Set(new PhysicsCubeNetState {
                Position = spawnPosition,
                Velocity = Vector3.zero,
                Rotation = rotation
            });
            SendEntity(ent);
            return ent;
        }
    }
}
