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
        guid: "ce5bcff2-7cdb-4b27-8c53-66c6639e28fa"
    )]
    public partial struct CampFlowViewState : IComponent, IComponentConfig<CampFlowViewState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted, IEquatable<CampFlowViewState>
    {
        [ReplicatedField]
        public ushort AnchorId;

        [ReplicatedField]
        public CampFlowStage Stage;

        public string ObjectiveDisplayName;

        public string HintDisplayName;

        [ReplicatedField]
        public bool CanToggleWorkerAssignment;

        [ReplicatedField]
        public bool CanOpenLoadoutPreparation;

        [ReplicatedField]
        public bool CanOpenExpeditionSelection;

        public SettlementAnchorId Anchor => new(AnchorId);

        public bool Equals(CampFlowViewState other)
        {
            return AnchorId == other.AnchorId
                   && Stage == other.Stage
                   && ObjectiveDisplayName == other.ObjectiveDisplayName
                   && HintDisplayName == other.HintDisplayName
                   && CanToggleWorkerAssignment == other.CanToggleWorkerAssignment
                   && CanOpenLoadoutPreparation == other.CanOpenLoadoutPreparation
                   && CanOpenExpeditionSelection == other.CanOpenExpeditionSelection;
        }

        public override bool Equals(object obj)
        {
            return obj is CampFlowViewState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                AnchorId,
                (byte)Stage,
                ObjectiveDisplayName,
                HintDisplayName,
                CanToggleWorkerAssignment,
                CanOpenLoadoutPreparation,
                CanOpenExpeditionSelection);
        }

        public ComponentTypeConfig<CampFlowViewState> Config() =>
            new(guid: new Guid("ce5bcff2-7cdb-4b27-8c53-66c6639e28fa"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(AnchorId);
            writer.WriteByte((byte)Stage);
            writer.WriteString16(ObjectiveDisplayName);
            writer.WriteString16(HintDisplayName);
            writer.WriteBool(CanToggleWorkerAssignment);
            writer.WriteBool(CanOpenLoadoutPreparation);
            writer.WriteBool(CanOpenExpeditionSelection);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            AnchorId = reader.ReadUshort();
            Stage = (CampFlowStage)reader.ReadByte();
            ObjectiveDisplayName = reader.ReadString16();
            HintDisplayName = reader.ReadString16();
            CanToggleWorkerAssignment = reader.ReadBool();
            CanOpenLoadoutPreparation = reader.ReadBool();
            CanOpenExpeditionSelection = reader.ReadBool();
        }
    }
}
