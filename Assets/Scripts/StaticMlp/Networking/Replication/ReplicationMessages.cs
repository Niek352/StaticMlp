using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Networking.Replication {
    public static class ComponentTypeIds {
        public const ushort CharacterNetState = 1;
        public const ushort NetworkIdentity = 2;
    }

    public readonly struct ComponentDelta {
        public readonly EntityGID Gid;
        public readonly ushort ComponentTypeId;
        public readonly byte[] Payload;

        public ComponentDelta(EntityGID gid, ushort componentTypeId, byte[] payload) {
            Gid = gid;
            ComponentTypeId = componentTypeId;
            Payload = payload;
        }
    }

    public sealed class ComponentBatch {
        public NetworkPeerId SourcePeer;
        public readonly List<ComponentDelta> Deltas = new();
    }

    public sealed class SpawnMessage {
        public EntityGID Gid;
        public NetworkPeerId Owner;
        public NetworkAuthority Authority;
        public ushort PrefabId;
        public readonly List<ComponentDelta> Components = new();
    }

    public readonly struct DespawnMessage {
        public readonly EntityGID Gid;

        public DespawnMessage(EntityGID gid) {
            Gid = gid;
        }
    }

    public readonly struct OwnershipChangedMessage {
        public readonly EntityGID Gid;
        public readonly NetworkPeerId NewOwner;
        public readonly NetworkAuthority Authority;

        public OwnershipChangedMessage(EntityGID gid, NetworkPeerId newOwner, NetworkAuthority authority) {
            Gid = gid;
            NewOwner = newOwner;
            Authority = authority;
        }
    }

    public sealed class NetworkEventMessage {
        public NetworkPeerId SourcePeer;
        public ushort EventTypeId;
        public byte[] Payload;
    }

    public sealed class NetInbox {
        public readonly List<SpawnMessage> Spawns = new();
        public readonly List<DespawnMessage> Despawns = new();
        public readonly List<OwnershipChangedMessage> OwnershipChanges = new();
        public readonly List<ComponentBatch> ComponentBatches = new();
        public readonly List<NetworkEventMessage> Events = new();

        public void Clear() {
            Spawns.Clear();
            Despawns.Clear();
            OwnershipChanges.Clear();
            ComponentBatches.Clear();
            Events.Clear();
        }
    }

    public sealed class OutgoingPacket {
        public NetworkPeerId Peer;
        public NetDelivery Delivery;
        public byte[] Payload;
    }

    public sealed class NetOutbox {
        public readonly List<OutgoingPacket> Packets = new();

        public void Enqueue(NetworkPeerId peer, byte[] payload, NetDelivery delivery) {
            Packets.Add(new OutgoingPacket {
                Peer = peer,
                Payload = payload,
                Delivery = delivery
            });
        }

        public void EnqueueComponentDelta(NetworkPeerId peer, ComponentDelta delta, NetDelivery delivery) {
            var batch = new ComponentBatch { SourcePeer = NetworkRuntime.LocalPeerId };
            batch.Deltas.Add(delta);
            Enqueue(peer, PacketCodec.EncodeComponentBatch(batch), delivery);
        }

        public void Clear() => Packets.Clear();
    }
}
