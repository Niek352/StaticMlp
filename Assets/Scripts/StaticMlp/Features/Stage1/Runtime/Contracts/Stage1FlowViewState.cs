using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [ReplicatedComponent(
        authority: ReplicationAuthority.Server,
        delivery: NetDelivery.ReliableSequenced,
        sendRate: 5,
        guid: "ce5bcff2-7cdb-4b27-8c53-66c6639e28fa"
    )]
    public partial struct Stage1FlowViewState : IComponent, IComponentConfig<Stage1FlowViewState>,
        ITrackableAdded, ITrackableChanged, ITrackableDeleted, IEquatable<Stage1FlowViewState>
    {
        [ReplicatedField]
        public ushort AnchorId;

        [ReplicatedField]
        public Stage1SettlementProgressStage Stage;

        [ReplicatedField]
        public Stage1FlowObjective Objective;

        [ReplicatedField]
        public Stage1FlowHint Hint;

        [ReplicatedField]
        public bool CanToggleWorkerAssignment;

        [ReplicatedField]
        public bool CanOpenBuildPreparation;

        [ReplicatedField]
        public bool CanOpenExpeditionSelection;

        public SettlementAnchorId Anchor => new(AnchorId);

        public bool Equals(Stage1FlowViewState other)
        {
            return AnchorId == other.AnchorId
                   && Stage == other.Stage
                   && Objective == other.Objective
                   && Hint == other.Hint
                   && CanToggleWorkerAssignment == other.CanToggleWorkerAssignment
                   && CanOpenBuildPreparation == other.CanOpenBuildPreparation
                   && CanOpenExpeditionSelection == other.CanOpenExpeditionSelection;
        }

        public override bool Equals(object obj)
        {
            return obj is Stage1FlowViewState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                AnchorId,
                (byte)Stage,
                (byte)Objective,
                (byte)Hint,
                CanToggleWorkerAssignment,
                CanOpenBuildPreparation,
                CanOpenExpeditionSelection);
        }

        public ComponentTypeConfig<Stage1FlowViewState> Config() =>
            new(guid: new Guid("ce5bcff2-7cdb-4b27-8c53-66c6639e28fa"));

        public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self)
            where TWorld : struct, IWorldType
        {
            writer.WriteUshort(AnchorId);
            writer.WriteByte((byte)Stage);
            writer.WriteByte((byte)Objective);
            writer.WriteByte((byte)Hint);
            writer.WriteBool(CanToggleWorkerAssignment);
            writer.WriteBool(CanOpenBuildPreparation);
            writer.WriteBool(CanOpenExpeditionSelection);
        }

        public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled)
            where TWorld : struct, IWorldType
        {
            AnchorId = reader.ReadUshort();
            Stage = (Stage1SettlementProgressStage)reader.ReadByte();
            Objective = (Stage1FlowObjective)reader.ReadByte();
            Hint = (Stage1FlowHint)reader.ReadByte();
            CanToggleWorkerAssignment = reader.ReadBool();
            CanOpenBuildPreparation = reader.ReadBool();
            CanOpenExpeditionSelection = reader.ReadBool();
        }
    }
}
