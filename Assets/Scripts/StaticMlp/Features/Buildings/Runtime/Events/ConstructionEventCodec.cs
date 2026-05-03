using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionEventCodec
    {
        public static void Register()
        {
            NetworkEventRegistry.Register<PlaceBuildingRequestEvent>(
                BuildingNetworkEventTypeIds.PlaceBuildingRequest,
                NetDelivery.ReliableSequenced,
                WritePlaceBuilding,
                TryReadPlaceBuilding);
            NetworkEventRegistry.Register<DepositConstructionResourcesRequestEvent>(
                BuildingNetworkEventTypeIds.DepositConstructionResourcesRequest,
                NetDelivery.ReliableSequenced,
                WriteDeposit,
                TryReadDeposit);
            NetworkEventRegistry.Register<BuildConstructionRequestEvent>(
                BuildingNetworkEventTypeIds.BuildConstructionRequest,
                NetDelivery.ReliableSequenced,
                WriteBuild,
                TryReadBuild);
        }

        private static byte[] WritePlaceBuilding(in PlaceBuildingRequestEvent evt) => Write(in evt);
        private static byte[] WriteDeposit(in DepositConstructionResourcesRequestEvent evt) => Write(in evt);
        private static byte[] WriteBuild(in BuildConstructionRequestEvent evt) => Write(in evt);

        public static byte[] Write(in PlaceBuildingRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(34);
            writer.WriteUshort(evt.BuildingId);
            writer.WriteFloat(evt.Position.x, evt.Position.y, evt.Position.z);
            writer.WriteFloat(evt.Rotation.x, evt.Rotation.y, evt.Rotation.z, evt.Rotation.w);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryReadPlaceBuilding(byte[] payload, out PlaceBuildingRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new PlaceBuildingRequestEvent(
                    reader.ReadUshort(),
                    new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                    new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()));
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }

        public static byte[] Write(in DepositConstructionResourcesRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(16);
            writer.WriteUlong(evt.Site.Raw);
            writer.WriteInt(evt.Wood);
            writer.WriteInt(evt.Stone);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryReadDeposit(byte[] payload, out DepositConstructionResourcesRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new DepositConstructionResourcesRequestEvent(
                    new EntityGID(reader.ReadUlong()),
                    reader.ReadInt(),
                    reader.ReadInt());
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }

        public static byte[] Write(in BuildConstructionRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(12);
            writer.WriteUlong(evt.Site.Raw);
            writer.WriteFloat(evt.WorkAmount);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryReadBuild(byte[] payload, out BuildConstructionRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new BuildConstructionRequestEvent(
                    new EntityGID(reader.ReadUlong()),
                    reader.ReadFloat());
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }
    }
}
