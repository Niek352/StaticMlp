using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerReceiveClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var batch in inbox.ComponentBatches) {
                foreach (var delta in batch.Deltas) {
                    if (!delta.Gid.TryUnpack<ServerWT>(out var e))
                        continue;

                    if (!e.Has<ClientOwned>() || !e.Has<NetworkIdentity>())
                        continue;

                    ref readonly var net = ref e.Read<NetworkIdentity>();
                    if (net.Owner != batch.SourcePeer)
                        continue;

                    ReplicationRegistry.ApplyDelta(e, delta);
                    ServerRelayBuffer.Add(batch.SourcePeer, delta);
                }
            }
        }
    }

    public sealed class ServerOwnedReplicationCollectSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();

            foreach (var e in SW.Query<All<ServerOwned, NetworkedTag, NetworkIdentity>>().Entities()) {
                foreach (var peer in ServerPeerRegistry.Peers)
                    ReplicationRegistry.CollectDirty(e, outbox, peer);
            }
        }
    }

    public sealed class ServerRelayClientOwnedStateSystem : ISystem {
        public void Update() {
            ref var outbox = ref SW.GetResource<NetOutbox>();

            foreach (var item in ServerRelayBuffer.Items) {
                foreach (var observer in ServerPeerRegistry.Peers) {
                    if (observer == item.SourcePeer)
                        continue;

                    outbox.EnqueueComponentDelta(observer, item.Delta, NetDelivery.UnreliableSequenced);
                }
            }

            ServerRelayBuffer.Clear();
        }
    }

    public sealed class ClientSpawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var spawn in inbox.Spawns) {
                if (spawn.Gid.TryUnpack<ClientCoreWT>(out _))
                    continue;

                var e = CW.NewEntityByGID<Default>(spawn.Gid);
                e.Set(new NetworkIdentity {
                    Owner = spawn.Owner,
                    Authority = spawn.Authority,
                    PrefabId = spawn.PrefabId
                });
                e.Set<NetworkedTag>();

                PrefabRegistry.Apply(spawn.PrefabId, e);
                ReplicationRegistry.ApplyInitialState(e, spawn.Components);
                OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
            }
        }
    }

    public sealed class ClientDespawnApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var despawn in inbox.Despawns) {
                if (despawn.Gid.TryUnpack<ClientCoreWT>(out var e))
                    e.Destroy();
            }
        }
    }

    public sealed class ClientOwnershipApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var msg in inbox.OwnershipChanges) {
                if (!msg.Gid.TryUnpack<ClientCoreWT>(out var e))
                    continue;

                ref var net = ref e.Mut<NetworkIdentity>();
                net.Owner = msg.NewOwner;
                net.Authority = msg.Authority;
                OwnershipTags.ApplyForClient(e, msg.NewOwner, msg.Authority);
            }
        }
    }

    public sealed class ClientComponentDeltaApplySystem : ISystem {
        public void Update() {
            ref var inbox = ref CW.GetResource<NetInbox>();

            foreach (var batch in inbox.ComponentBatches) {
                foreach (var delta in batch.Deltas) {
                    if (!delta.Gid.TryUnpack<ClientCoreWT>(out var e))
                        continue;

                    if (e.Has<LocalOwned>())
                        continue;

                    ReplicationRegistry.ApplyDelta(e, delta);
                }
            }
        }
    }

    public sealed class ClientReplicationCollectSystem : ISystem {
        public void Update() {
            ref var outbox = ref CW.GetResource<NetOutbox>();

            foreach (var e in CW.Query<All<LocalOwned, NetworkedTag, NetworkIdentity>>().Entities())
                ReplicationRegistry.CollectDirty(e, outbox, new NetworkPeerId(0));
        }
    }
}
