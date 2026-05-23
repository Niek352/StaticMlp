using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CampFlow
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "8d47b71d-f6a0-4ecf-9b4e-b9a3d8fae6c1"
    )]
    public partial struct CampFlowProgression : IComponent, IComponentConfig<CampFlowProgression>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted
    {
        [ReplicatedField]
        public ushort AnchorId;

        [ReplicatedField]
        public CampFlowStage Stage;

        public SettlementAnchorId Anchor => new(AnchorId);

        public bool CanAdvanceTo(CampFlowStage nextStage)
        {
            return (byte)nextStage == (byte)Stage + 1;
        }

        public void AdvanceTo(CampFlowStage nextStage)
        {
            if (!CanAdvanceTo(nextStage))
            {
                throw new InvalidOperationException(
                    $"Invalid camp flow progression transition from {Stage} to {nextStage} for anchor {AnchorId}.");
            }

            Stage = nextStage;
        }

        public ComponentTypeConfig<CampFlowProgression> Config() =>
            new(guid: new Guid("8d47b71d-f6a0-4ecf-9b4e-b9a3d8fae6c1"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(AnchorId);
            writer.WriteByte((byte)Stage);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            AnchorId = reader.ReadUshort();
            Stage = (CampFlowStage)reader.ReadByte();
        }
    }
}
