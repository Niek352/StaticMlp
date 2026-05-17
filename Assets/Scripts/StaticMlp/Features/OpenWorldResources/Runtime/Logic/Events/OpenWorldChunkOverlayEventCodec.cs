using System;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public static class OpenWorldChunkOverlayEventCodec
    {
        private const int MAX_OVERLAY_ITEMS_PER_PACKET = 4096;

        public static void Register()
        {
            NetworkEventRegistry.Register<OpenWorldChunkOverlayRequest>(
                OpenWorldResourceNetworkEventIds.ChunkOverlayRequest,
                NetDelivery.ReliableSequenced,
                16,
                WriteRequest,
                ReadRequest);
            NetworkEventRegistry.Register<OpenWorldChunkOverlayAbsolute>(
                OpenWorldResourceNetworkEventIds.ChunkOverlayAbsolute,
                NetDelivery.ReliableSequenced,
                64,
                WriteAbsolute,
                ReadAbsolute);
            NetworkEventRegistry.Register<OpenWorldChunkOverlayDelta>(
                OpenWorldResourceNetworkEventIds.ChunkOverlayDelta,
                NetDelivery.ReliableSequenced,
                64,
                WriteDelta,
                ReadDelta);
            NetworkEventRegistry.Register<OpenWorldChunkOverlayAck>(
                OpenWorldResourceNetworkEventIds.ChunkOverlayAck,
                NetDelivery.ReliableSequenced,
                16,
                WriteAck,
                ReadAck);
            NetworkEventRegistry.Register<TryHarvestOpenWorldResourceCommand>(
                OpenWorldResourceNetworkEventIds.TryHarvestResource,
                NetDelivery.ReliableSequenced,
                24,
                WriteHarvestCommand,
                ReadHarvestCommand);
        }

        private static void WriteRequest(ref NetworkWriter writer, in OpenWorldChunkOverlayRequest evt)
        {
            writer.WriteWorldChunkId(evt.ChunkId);
            writer.WriteUint(evt.KnownRevision);
        }

        private static OpenWorldChunkOverlayRequest ReadRequest(ref NetworkReader reader)
        {
            return new OpenWorldChunkOverlayRequest
            {
                ChunkId = reader.ReadWorldChunkId(),
                KnownRevision = reader.ReadUint()
            };
        }

        private static void WriteAbsolute(ref NetworkWriter writer, in OpenWorldChunkOverlayAbsolute evt)
        {
            writer.WriteWorldChunkId(evt.ChunkId);
            writer.WriteUint(evt.Revision);
            writer.WriteInt(evt.ResourceStates.Length);
            for (var i = 0; i < evt.ResourceStates.Length; i++)
                WriteResourceState(ref writer, evt.ResourceStates[i]);
        }

        private static OpenWorldChunkOverlayAbsolute ReadAbsolute(ref NetworkReader reader)
        {
            var chunkId = reader.ReadWorldChunkId();
            var revision = reader.ReadUint();
            var count = ReadOverlayItemCount(ref reader);
            var states = new OpenWorldResourceOverlayState[count];
            for (var i = 0; i < states.Length; i++)
                states[i] = ReadResourceState(ref reader);

            return new OpenWorldChunkOverlayAbsolute
            {
                ChunkId = chunkId,
                Revision = revision,
                ResourceStates = states
            };
        }

        private static void WriteDelta(ref NetworkWriter writer, in OpenWorldChunkOverlayDelta evt)
        {
            writer.WriteWorldChunkId(evt.ChunkId);
            writer.WriteUint(evt.BasisRevision);
            writer.WriteUint(evt.Revision);
            writer.WriteInt(evt.ResourceDeltas.Length);
            for (var i = 0; i < evt.ResourceDeltas.Length; i++)
                WriteResourceDelta(ref writer, evt.ResourceDeltas[i]);
        }

        private static OpenWorldChunkOverlayDelta ReadDelta(ref NetworkReader reader)
        {
            var chunkId = reader.ReadWorldChunkId();
            var basisRevision = reader.ReadUint();
            var revision = reader.ReadUint();
            var count = ReadOverlayItemCount(ref reader);
            var deltas = new OpenWorldResourceOverlayDelta[count];
            for (var i = 0; i < deltas.Length; i++)
                deltas[i] = ReadResourceDelta(ref reader);

            return new OpenWorldChunkOverlayDelta
            {
                ChunkId = chunkId,
                BasisRevision = basisRevision,
                Revision = revision,
                ResourceDeltas = deltas
            };
        }

        private static void WriteAck(ref NetworkWriter writer, in OpenWorldChunkOverlayAck evt)
        {
            writer.WriteWorldChunkId(evt.ChunkId);
            writer.WriteUint(evt.Revision);
        }

        private static OpenWorldChunkOverlayAck ReadAck(ref NetworkReader reader)
        {
            return new OpenWorldChunkOverlayAck
            {
                ChunkId = reader.ReadWorldChunkId(),
                Revision = reader.ReadUint()
            };
        }

        private static void WriteHarvestCommand(ref NetworkWriter writer, in TryHarvestOpenWorldResourceCommand evt)
        {
            writer.WriteLong(evt.PlacementId);
            writer.WriteUshort(evt.ToolId);
            writer.WriteInt(evt.HitPointXQ);
            writer.WriteInt(evt.HitPointYQ);
            writer.WriteInt(evt.HitPointZQ);
        }

        private static TryHarvestOpenWorldResourceCommand ReadHarvestCommand(ref NetworkReader reader)
        {
            return new TryHarvestOpenWorldResourceCommand
            {
                PlacementId = reader.ReadLong(),
                ToolId = reader.ReadUshort(),
                HitPointXQ = reader.ReadInt(),
                HitPointYQ = reader.ReadInt(),
                HitPointZQ = reader.ReadInt()
            };
        }

        private static void WriteResourceState(ref NetworkWriter writer, OpenWorldResourceOverlayState state)
        {
            writer.WriteLong(state.PlacementId);
            writer.WriteUshort(state.KindIdValue);
            writer.WriteUshort(state.RemainingAmount);
            writer.WriteByte((byte)state.Flags);
            writer.WriteUint(state.RespawnTick);
        }

        private static OpenWorldResourceOverlayState ReadResourceState(ref NetworkReader reader)
        {
            return new OpenWorldResourceOverlayState
            {
                PlacementId = reader.ReadLong(),
                KindIdValue = reader.ReadUshort(),
                RemainingAmount = reader.ReadUshort(),
                Flags = (OpenWorldResourceOverlayFlags)reader.ReadByte(),
                RespawnTick = reader.ReadUint()
            };
        }

        private static void WriteResourceDelta(ref NetworkWriter writer, OpenWorldResourceOverlayDelta delta)
        {
            writer.WriteLong(delta.PlacementId);
            writer.WriteUshort(delta.RemainingAmount);
            writer.WriteByte((byte)delta.Flags);
            writer.WriteUint(delta.RespawnTick);
        }

        private static OpenWorldResourceOverlayDelta ReadResourceDelta(ref NetworkReader reader)
        {
            return new OpenWorldResourceOverlayDelta
            {
                PlacementId = reader.ReadLong(),
                RemainingAmount = reader.ReadUshort(),
                Flags = (OpenWorldResourceOverlayFlags)reader.ReadByte(),
                RespawnTick = reader.ReadUint()
            };
        }

        private static int ReadOverlayItemCount(ref NetworkReader reader)
        {
            var count = reader.ReadInt();
            if (count < 0 || count > MAX_OVERLAY_ITEMS_PER_PACKET)
                throw new InvalidOperationException($"Invalid open world overlay item count: {count}.");

            return count;
        }
    }
}
