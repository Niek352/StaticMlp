using System;
using System.Collections.Generic;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Diagnostics {
    public static class NetworkTrafficProfiler {
        private const int MaxSamples = 512;

        private static readonly object Sync = new();
        private static readonly List<Sample> Samples = new(MaxSamples);
        private static readonly Dictionary<PacketKey, Aggregate> PacketAggregates = new();
        private static readonly Dictionary<ComponentKey, Aggregate> ComponentAggregates = new();

        public static bool Enabled;
        public static Func<ushort, string> ComponentNameResolver;
        public static Func<ushort, NetDelivery> ComponentDeliveryResolver;

        public static void RecordOutgoingPacket(NetworkPeerId peer, NetDelivery delivery, ReadOnlySpan<byte> payload, bool success = true) {
            if (!Enabled)
                return;

            RecordPacket(NetworkTrafficDirection.Sent, peer, delivery, payload, success);
        }

        public static void RecordIncomingPacket(NetworkPeerId peer, ReadOnlySpan<byte> payload) {
            if (!Enabled)
                return;

            RecordPacket(NetworkTrafficDirection.Received, peer, NetDelivery.Unreliable, payload, true);
        }

        public static Snapshot GetSnapshot(double windowSeconds) {
            lock (Sync) {
                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-Math.Max(0.1d, windowSeconds));
                var recent = new List<Sample>(Samples.Count);
                var packetAggregates = new Dictionary<PacketKey, Aggregate>();
                var componentAggregates = new Dictionary<ComponentKey, Aggregate>();
                var sentBytes = 0;
                var receivedBytes = 0;
                var sentPackets = 0;
                var receivedPackets = 0;

                foreach (var sample in Samples) {
                    if (sample.UtcTime < windowStart)
                        continue;

                    recent.Add(sample);
                    if (!sample.IsPacketSample)
                    {
                        var componentKey = new ComponentKey(
                            sample.Direction,
                            sample.Peer,
                            sample.Delivery,
                            sample.ComponentTypeId,
                            sample.Name,
                            sample.Channel);

                        if (!componentAggregates.TryGetValue(componentKey, out var componentAggregate))
                            componentAggregate = new Aggregate();

                        componentAggregate.Packets++;
                        componentAggregate.Bytes += sample.Bytes;
                        componentAggregate.LastUtcTime = sample.UtcTime;
                        if (!sample.Success)
                            componentAggregate.FailedPackets++;

                        componentAggregates[componentKey] = componentAggregate;
                        continue;
                    }

                    if (sample.Direction == NetworkTrafficDirection.Sent) {
                        sentBytes += sample.Bytes;
                        sentPackets++;
                    } else {
                        receivedBytes += sample.Bytes;
                        receivedPackets++;
                    }

                    var packetKey = new PacketKey(
                        sample.Direction,
                        sample.Peer,
                        sample.Delivery,
                        sample.Name);

                    if (!packetAggregates.TryGetValue(packetKey, out var packetAggregate))
                        packetAggregate = new Aggregate();

                    packetAggregate.Packets++;
                    packetAggregate.Bytes += sample.Bytes;
                    packetAggregate.LastUtcTime = sample.UtcTime;
                    if (!sample.Success)
                        packetAggregate.FailedPackets++;

                    packetAggregates[packetKey] = packetAggregate;
                }

                var packetRows = new List<AggregateRow>(packetAggregates.Count);
                foreach (var pair in packetAggregates) {
                    var aggregate = pair.Value;
                    packetRows.Add(new AggregateRow(
                        pair.Key.Direction,
                        pair.Key.Peer,
                        pair.Key.Delivery,
                        pair.Key.Name,
                        aggregate.Packets,
                        aggregate.Bytes,
                        aggregate.FailedPackets,
                        aggregate.LastUtcTime
                    ));
                }

                var componentRows = new List<ComponentAggregateRow>(componentAggregates.Count);
                foreach (var pair in componentAggregates) {
                    var aggregate = pair.Value;
                    componentRows.Add(new ComponentAggregateRow(
                        pair.Key.Direction,
                        pair.Key.Peer,
                        pair.Key.Delivery,
                        pair.Key.ComponentTypeId,
                        pair.Key.ComponentName,
                        pair.Key.Channel,
                        aggregate.Packets,
                        aggregate.Bytes,
                        aggregate.FailedPackets,
                        aggregate.LastUtcTime
                    ));
                }

                packetRows.Sort(CompareAggregateRows);
                componentRows.Sort(CompareComponentAggregateRows);
                recent.Sort(CompareSamplesDescending);

                return new Snapshot(
                    Enabled,
                    windowSeconds,
                    sentBytes,
                    receivedBytes,
                    sentPackets,
                    receivedPackets,
                    recent,
                    packetRows,
                    componentRows
                );
            }
        }

        public static void Clear() {
            lock (Sync) {
                Samples.Clear();
                PacketAggregates.Clear();
                ComponentAggregates.Clear();
            }
        }

        private static void RecordPacket(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, ReadOnlySpan<byte> payload, bool success) {
            var bytes = payload.Length;
            var packetType = PacketTypeName(payload);
            var sample = new Sample(DateTime.UtcNow, direction, peer, delivery, packetType, bytes, success, string.Empty, 0);

            lock (Sync) {
                TrimSamplesIfNeeded();
                Samples.Add(sample);

                var key = new PacketKey(direction, peer, delivery, packetType);
                if (!PacketAggregates.TryGetValue(key, out var aggregate))
                    aggregate = new Aggregate();

                aggregate.Packets++;
                aggregate.Bytes += bytes;
                aggregate.LastUtcTime = sample.UtcTime;
                if (!success)
                    aggregate.FailedPackets++;

                PacketAggregates[key] = aggregate;
            }
        }

        private static void RecordComponent(
            NetworkTrafficDirection direction,
            NetworkPeerId peer,
            NetDelivery delivery,
            ushort componentTypeId,
            int bytes,
            string channel) {
            var componentName = ResolveComponentName(componentTypeId);
            var sample = new Sample(DateTime.UtcNow, direction, peer, delivery, componentName, bytes, true, channel, componentTypeId);

            lock (Sync) {
                TrimSamplesIfNeeded();
                Samples.Add(sample);

                var key = new ComponentKey(direction, peer, delivery, componentTypeId, componentName, channel);
                if (!ComponentAggregates.TryGetValue(key, out var aggregate))
                    aggregate = new Aggregate();

                aggregate.Packets++;
                aggregate.Bytes += bytes;
                aggregate.LastUtcTime = sample.UtcTime;
                ComponentAggregates[key] = aggregate;
            }
        }

        private static void TrimSamplesIfNeeded() {
            if (Samples.Count == MaxSamples)
                Samples.RemoveAt(0);
        }

        private static string ResolveComponentName(ushort componentTypeId) {
            return ComponentNameResolver != null
                ? ComponentNameResolver(componentTypeId)
                : $"Component({componentTypeId})";
        }

        private static NetDelivery ResolveComponentDelivery(ushort componentTypeId) {
            return ComponentDeliveryResolver != null
                ? ComponentDeliveryResolver(componentTypeId)
                : NetDelivery.Unreliable;
        }

        private static string PacketTypeName(ReadOnlySpan<byte> payload) {
            if (payload.Length == 0)
                return "Empty";

            var value = payload[0];
            return Enum.IsDefined(typeof(NetPacketType), value)
                ? ((NetPacketType)value).ToString()
                : $"Unknown({value})";
        }

        private static int CompareAggregateRows(AggregateRow left, AggregateRow right) {
            var bytes = right.Bytes.CompareTo(left.Bytes);
            return bytes != 0 ? bytes : right.Packets.CompareTo(left.Packets);
        }

        private static int CompareComponentAggregateRows(ComponentAggregateRow left, ComponentAggregateRow right) {
            var bytes = right.Bytes.CompareTo(left.Bytes);
            return bytes != 0 ? bytes : right.Packets.CompareTo(left.Packets);
        }

        private static int CompareSamplesDescending(Sample left, Sample right) {
            return right.UtcTime.CompareTo(left.UtcTime);
        }

        private readonly struct PacketKey : IEquatable<PacketKey> {
            public readonly NetworkTrafficDirection Direction;
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly string Name;

            public PacketKey(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, string name) {
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                Name = name;
            }

            public bool Equals(PacketKey other) {
                return Direction == other.Direction &&
                       Peer == other.Peer &&
                       Delivery == other.Delivery &&
                       Name == other.Name;
            }

            public override bool Equals(object obj) {
                return obj is PacketKey other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hash = (int)Direction;
                    hash = (hash * 397) ^ Peer.GetHashCode();
                    hash = (hash * 397) ^ (int)Delivery;
                    hash = (hash * 397) ^ Name.GetHashCode();
                    return hash;
                }
            }
        }

        private readonly struct ComponentKey : IEquatable<ComponentKey> {
            public readonly NetworkTrafficDirection Direction;
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly ushort ComponentTypeId;
            public readonly string ComponentName;
            public readonly string Channel;

            public ComponentKey(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, ushort componentTypeId, string componentName, string channel) {
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                ComponentTypeId = componentTypeId;
                ComponentName = componentName;
                Channel = channel;
            }

            public bool Equals(ComponentKey other) {
                return Direction == other.Direction &&
                       Peer == other.Peer &&
                       Delivery == other.Delivery &&
                       ComponentTypeId == other.ComponentTypeId &&
                       ComponentName == other.ComponentName &&
                       Channel == other.Channel;
            }

            public override bool Equals(object obj) {
                return obj is ComponentKey other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hash = (int)Direction;
                    hash = (hash * 397) ^ Peer.GetHashCode();
                    hash = (hash * 397) ^ (int)Delivery;
                    hash = (hash * 397) ^ ComponentTypeId.GetHashCode();
                    hash = (hash * 397) ^ ComponentName.GetHashCode();
                    hash = (hash * 397) ^ Channel.GetHashCode();
                    return hash;
                }
            }
        }

        private struct Aggregate {
            public int Packets;
            public int Bytes;
            public int FailedPackets;
            public DateTime LastUtcTime;
        }

        public readonly struct AggregateRow {
            public readonly NetworkTrafficDirection Direction;
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly string Name;
            public readonly int Packets;
            public readonly int Bytes;
            public readonly int FailedPackets;
            public readonly DateTime LastUtcTime;

            public AggregateRow(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, string name, int packets, int bytes, int failedPackets, DateTime lastUtcTime) {
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                Name = name;
                Packets = packets;
                Bytes = bytes;
                FailedPackets = failedPackets;
                LastUtcTime = lastUtcTime;
            }
        }

        public readonly struct ComponentAggregateRow {
            public readonly NetworkTrafficDirection Direction;
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly ushort ComponentTypeId;
            public readonly string ComponentName;
            public readonly string Channel;
            public readonly int Packets;
            public readonly int Bytes;
            public readonly int FailedPackets;
            public readonly DateTime LastUtcTime;

            public ComponentAggregateRow(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, ushort componentTypeId, string componentName, string channel, int packets, int bytes, int failedPackets, DateTime lastUtcTime) {
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                ComponentTypeId = componentTypeId;
                ComponentName = componentName;
                Channel = channel;
                Packets = packets;
                Bytes = bytes;
                FailedPackets = failedPackets;
                LastUtcTime = lastUtcTime;
            }
        }

        public readonly struct Sample {
            public readonly DateTime UtcTime;
            public readonly NetworkTrafficDirection Direction;
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly string Name;
            public readonly int Bytes;
            public readonly bool Success;
            public readonly string Channel;
            public readonly ushort ComponentTypeId;
            public readonly bool IsPacketSample;

            public Sample(DateTime utcTime, NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, string name, int bytes, bool success, string channel, ushort componentTypeId) {
                UtcTime = utcTime;
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                Name = name;
                Bytes = bytes;
                Success = success;
                Channel = channel;
                ComponentTypeId = componentTypeId;
                IsPacketSample = string.IsNullOrEmpty(channel);
            }
        }

        public readonly struct Snapshot {
            public readonly bool Enabled;
            public readonly double WindowSeconds;
            public readonly int SentBytesInWindow;
            public readonly int ReceivedBytesInWindow;
            public readonly int SentPacketsInWindow;
            public readonly int ReceivedPacketsInWindow;
            public readonly List<Sample> RecentSamples;
            public readonly List<AggregateRow> PacketAggregates;
            public readonly List<ComponentAggregateRow> ComponentAggregates;

            public Snapshot(bool enabled, double windowSeconds, int sentBytesInWindow, int receivedBytesInWindow, int sentPacketsInWindow, int receivedPacketsInWindow, List<Sample> recentSamples, List<AggregateRow> packetAggregates, List<ComponentAggregateRow> componentAggregates) {
                Enabled = enabled;
                WindowSeconds = windowSeconds;
                SentBytesInWindow = sentBytesInWindow;
                ReceivedBytesInWindow = receivedBytesInWindow;
                SentPacketsInWindow = sentPacketsInWindow;
                ReceivedPacketsInWindow = receivedPacketsInWindow;
                RecentSamples = recentSamples;
                PacketAggregates = packetAggregates;
                ComponentAggregates = componentAggregates;
            }
        }
    }
}
