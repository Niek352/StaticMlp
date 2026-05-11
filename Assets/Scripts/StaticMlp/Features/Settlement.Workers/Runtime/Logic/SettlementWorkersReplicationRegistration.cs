using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkersReplicationRegistration
    {
        public static void Register()
        {
            ReplicationRegistry.RegisterComponent<SettlementWorkerIdentity>(
                SettlementWorkerIdentityReplication.TYPE_ID,
                SettlementWorkerIdentityReplication.AUTHORITY,
                SettlementWorkerIdentityReplication.AUDIENCE,
                SettlementWorkerIdentityReplication.DELIVERY,
                SettlementWorkerIdentityReplication.CreateDelta,
                SettlementWorkerIdentityReplication.Read);

            ReplicationRegistry.RegisterComponent<SettlementWorkerAssignment>(
                SettlementWorkerAssignmentReplication.TYPE_ID,
                SettlementWorkerAssignmentReplication.AUTHORITY,
                SettlementWorkerAssignmentReplication.AUDIENCE,
                SettlementWorkerAssignmentReplication.DELIVERY,
                SettlementWorkerAssignmentReplication.CreateDelta,
                SettlementWorkerAssignmentReplication.Read);

            ReplicationRegistry.RegisterComponent<SettlementCampBuilderJobState>(
                SettlementCampBuilderJobStateReplication.TYPE_ID,
                SettlementCampBuilderJobStateReplication.AUTHORITY,
                SettlementCampBuilderJobStateReplication.AUDIENCE,
                SettlementCampBuilderJobStateReplication.DELIVERY,
                SettlementCampBuilderJobStateReplication.CreateDelta,
                SettlementCampBuilderJobStateReplication.Read);

            ReplicationRegistry.RegisterComponent<SettlementWorkerSummary>(
                SettlementWorkerSummaryReplication.TYPE_ID,
                SettlementWorkerSummaryReplication.AUTHORITY,
                SettlementWorkerSummaryReplication.AUDIENCE,
                SettlementWorkerSummaryReplication.DELIVERY,
                SettlementWorkerSummaryReplication.CreateDelta,
                SettlementWorkerSummaryReplication.Read);
        }
    }
}
