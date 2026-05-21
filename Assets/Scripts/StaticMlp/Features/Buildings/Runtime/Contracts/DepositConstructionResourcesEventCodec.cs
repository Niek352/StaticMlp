using System;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public static class DepositConstructionResourcesEventCodec
    {
        private const int MAX_RESOURCES_PER_DEPOSIT = 64;

        public static void Register()
        {
            NetworkEventRegistry.Register<DepositConstructionResourcesRequestEvent>(
                DepositConstructionResourcesRequestEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                32,
                WriteRequest,
                ReadRequest);
            NetworkEventRegistry.Register<DepositConstructionResourcesResultEvent>(
                DepositConstructionResourcesResultEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                32,
                WriteResult,
                ReadResult);
        }

        private static void WriteRequest(ref NetworkWriter writer, in DepositConstructionResourcesRequestEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteEntityGid(evt.Site);
            WriteResources(ref writer, evt.Resources);
        }

        private static DepositConstructionResourcesRequestEvent ReadRequest(ref NetworkReader reader)
        {
            return new DepositConstructionResourcesRequestEvent
            {
                RequestId = reader.ReadRequestId(),
                Site = reader.ReadEntityGid(),
                Resources = ReadResources(ref reader)
            };
        }

        private static void WriteResult(ref NetworkWriter writer, in DepositConstructionResourcesResultEvent evt)
        {
            writer.WriteRequestId(evt.RequestId);
            writer.WriteByte((byte)evt.Status);
            writer.WriteEntityGid(evt.Site);
            WriteResources(ref writer, evt.AcceptedResources);
        }

        private static DepositConstructionResourcesResultEvent ReadResult(ref NetworkReader reader)
        {
            return new DepositConstructionResourcesResultEvent
            {
                RequestId = reader.ReadRequestId(),
                Status = (RequestStatus)reader.ReadByte(),
                Site = reader.ReadEntityGid(),
                AcceptedResources = ReadResources(ref reader)
            };
        }

        private static void WriteResources(ref NetworkWriter writer, ResourceAmount[] resources)
        {
            if (resources == null)
                throw new InvalidOperationException("Deposit construction resources payload cannot be null.");

            ValidateResources(resources);
            writer.WriteInt(resources.Length);
            for (var i = 0; i < resources.Length; i++)
            {
                writer.WriteUshort(resources[i].Id.Value);
                writer.WriteInt(resources[i].Amount);
            }
        }

        private static ResourceAmount[] ReadResources(ref NetworkReader reader)
        {
            var count = reader.ReadInt();
            if (count < 0 || count > MAX_RESOURCES_PER_DEPOSIT)
                throw new InvalidOperationException($"Invalid deposit construction resources count {count}.");

            var resources = new ResourceAmount[count];
            for (var i = 0; i < count; i++)
                resources[i] = new ResourceAmount(new ResourceId(reader.ReadUshort()), reader.ReadInt());

            ValidateResources(resources);
            return resources;
        }

        private static void ValidateResources(ResourceAmount[] resources)
        {
            for (var i = 0; i < resources.Length; i++)
            {
                var resource = resources[i];
                ResourceCatalog.Get(resource.Id);
                if (resource.Amount < 0)
                    throw new InvalidOperationException($"Deposit construction resource id {resource.Id.Value} has negative amount {resource.Amount}.");

                for (var j = i + 1; j < resources.Length; j++)
                {
                    if (resource.Id == resources[j].Id)
                        throw new InvalidOperationException($"Duplicate deposit construction resource id {resource.Id.Value}.");
                }
            }
        }
    }
}
