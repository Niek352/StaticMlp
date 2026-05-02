using System;
using System.Collections.Generic;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Diagnostics {
    public static class NetworkTrafficProfiler {
        private const int MaxSamples = 512;
        private static readonly object Sync = new();
        private static readonly List<Sample> Samples = new(MaxSamples);
        private static readonly Dictionary<Key, Aggregate> Aggregates = new();

        public static bool Enabled = true;

        public static void RecordSent(NetworkPeerId peer, NetDelivery delivery, byte[] payload, bool success) {
            if (!Enabled)
                return;

            Record(NetworkTrafficDirection.Sent, peer, delivery, payload, success);
        }

        public static void RecordReceived(NetworkPeerId peer, byte[] payload) {
            if (!Enabled)
                return;

            Record(NetworkTrafficDirection.Received, peer, NetDelivery.Unreliable, payload, true);
        }

        public static Snapshot GetSnapshot(double windowSeconds) {
            lock (Sync) {
                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-Math.Max(0.1d, windowSeconds));
                var recent = new List<Sample>(Samples.Count);
                var sentBytes = 0;
                var receivedBytes = 0;
                var sentPackets = 0;
                var receivedPackets = 0;

                foreach (var sample in Samples) {
                    if (sample.UtcTime < windowStart)
                        continue;

                    recent.Add(sample);
                    if (sample.Direction == NetworkTrafficDirection.Sent) {
                        sentBytes += sample.Bytes;
                        sentPackets++;
                    } else {
                        receivedBytes += sample.Bytes;
                        receivedPackets++;
                    }
                }

                var aggregates = new List<AggregateRow>(Aggregates.Count);
                foreach (var pair in Aggregates) {
                    var aggregate = pair.Value;
                    aggregates.Add(new AggregateRow(
                        pair.Key.Direction,
                        pair.Key.Peer,
                        pair.Key.Delivery,
                        pair.Key.PacketType,
                        aggregate.Packets,
                        aggregate.Bytes,
                        aggregate.FailedPackets,
                        aggregate.LastUtcTime
                    ));
                }

                aggregates.Sort(CompareAggregateRows);
                recent.Sort(CompareSamplesDescending);

                return new Snapshot(
                    Enabled,
                    windowSeconds,
                    sentBytes,
                    receivedBytes,
                    sentPackets,
                    receivedPackets,
                    recent,
                    aggregates
                );
            }
        }

        public static void Clear() {
            lock (Sync) {
                Samples.Clear();
                Aggregates.Clear();
            }
        }

        private static void Record(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, byte[] payload, bool success) {
            var bytes = payload?.Length ?? 0;
            var packetType = PacketTypeName(payload);
            var sample = new Sample(DateTime.UtcNow, direction, peer, delivery, packetType, bytes, success);

            lock (Sync) {
                if (Samples.Count == MaxSamples)
                    Samples.RemoveAt(0);

                Samples.Add(sample);

                var key = new Key(direction, peer, delivery, packetType);
                if (!Aggregates.TryGetValue(key, out var aggregate))
                    aggregate = new Aggregate();

                aggregate.Packets++;
                aggregate.Bytes += bytes;
                aggregate.LastUtcTime = sample.UtcTime;
                if (!success)
                    aggregate.FailedPackets++;

                Aggregates[key] = aggregate;
            }
        }

        private static string PacketTypeName(byte[] payload) {
            if (payload == null || payload.Length == 0)
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

        private static int CompareSamplesDescending(Sample left, Sample right) {
            return right.UtcTime.CompareTo(left.UtcTime);
        }

        private readonly struct Key : IEquatable<Key> {
            public readonly NetworkTrafficDirection Direction;
            public readonly NetworkPeerId Peer;
            public readonly NetDelivery Delivery;
            public readonly string PacketType;

            public Key(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, string packetType) {
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                PacketType = packetType;
            }

            public bool Equals(Key other) {
                return Direction == other.Direction &&
                       Peer == other.Peer &&
                       Delivery == other.Delivery &&
                       PacketType == other.PacketType;
            }

            public override bool Equals(object obj) {
                return obj is Key other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hash = (int)Direction;
                    hash = (hash * 397) ^ Peer.GetHashCode();
                    hash = (hash * 397) ^ (int)Delivery;
                    hash = (hash * 397) ^ PacketType.GetHashCode();
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
            public readonly string PacketType;
            public readonly int Packets;
            public readonly int Bytes;
            public readonly int FailedPackets;
            public readonly DateTime LastUtcTime;

            public AggregateRow(NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, string packetType, int packets, int bytes, int failedPackets, DateTime lastUtcTime) {
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                PacketType = packetType;
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
            public readonly string PacketType;
            public readonly int Bytes;
            public readonly bool Success;

            public Sample(DateTime utcTime, NetworkTrafficDirection direction, NetworkPeerId peer, NetDelivery delivery, string packetType, int bytes, bool success) {
                UtcTime = utcTime;
                Direction = direction;
                Peer = peer;
                Delivery = delivery;
                PacketType = packetType;
                Bytes = bytes;
                Success = success;
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
            public readonly List<AggregateRow> Aggregates;

            public Snapshot(bool enabled, double windowSeconds, int sentBytesInWindow, int receivedBytesInWindow, int sentPacketsInWindow, int receivedPacketsInWindow, List<Sample> recentSamples, List<AggregateRow> aggregates) {
                Enabled = enabled;
                WindowSeconds = windowSeconds;
                SentBytesInWindow = sentBytesInWindow;
                ReceivedBytesInWindow = receivedBytesInWindow;
                SentPacketsInWindow = sentPacketsInWindow;
                ReceivedPacketsInWindow = receivedPacketsInWindow;
                RecentSamples = recentSamples;
                Aggregates = aggregates;
            }
        }
    }
}
