using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkersReplicationRegistration
    {
        public static void Register()
        {
            ReplicationRegistry.RegisterComponent<SettlementWorkerIdentity>(
                SettlementWorkerIdentityReplication.TypeId,
                SettlementWorkerIdentityReplication.Authority,
                SettlementWorkerIdentityReplication.Audience,
                SettlementWorkerIdentityReplication.Delivery,
                SettlementWorkerIdentityReplication.CreateDelta,
                SettlementWorkerIdentityReplication.Read);

            ReplicationRegistry.RegisterComponent<SettlementWorkerAssignment>(
                SettlementWorkerAssignmentReplication.TypeId,
                SettlementWorkerAssignmentReplication.Authority,
                SettlementWorkerAssignmentReplication.Audience,
                SettlementWorkerAssignmentReplication.Delivery,
                SettlementWorkerAssignmentReplication.CreateDelta,
                SettlementWorkerAssignmentReplication.Read);

            ReplicationRegistry.RegisterComponent<SettlementCampBuilderJobState>(
                SettlementCampBuilderJobStateReplication.TypeId,
                SettlementCampBuilderJobStateReplication.Authority,
                SettlementCampBuilderJobStateReplication.Audience,
                SettlementCampBuilderJobStateReplication.Delivery,
                SettlementCampBuilderJobStateReplication.CreateDelta,
                SettlementCampBuilderJobStateReplication.Read);

            ReplicationRegistry.RegisterComponent<SettlementWorkerSummary>(
                SettlementWorkerSummaryReplication.TypeId,
                SettlementWorkerSummaryReplication.Authority,
                SettlementWorkerSummaryReplication.Audience,
                SettlementWorkerSummaryReplication.Delivery,
                SettlementWorkerSummaryReplication.CreateDelta,
                SettlementWorkerSummaryReplication.Read);
        }
    }
}
