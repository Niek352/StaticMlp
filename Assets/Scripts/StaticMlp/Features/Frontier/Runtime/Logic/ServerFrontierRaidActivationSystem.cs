using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierRaidActivationSystem : ISystem
    {
        private static readonly Vector3 RAID_SPAWN_OFFSET = new(0f, 0f, 8f);

        public void Update()
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            foreach (var anchor in SW.Query<All<CampFlowProgression, SettlementAnchorLocation, RaidScheduleState, ThreatState>>().Entities())
            {
                ref readonly var raidSchedule = ref anchor.Read<RaidScheduleState>();
                if (raidSchedule.Status != RaidScheduleStatus.Pending
                    || raidSchedule.ActivateAtTick > simulationTime.ServerTick)
                {
                    continue;
                }

                var raid = RaidCatalog.Get(raidSchedule.RaidId);
                var origin = anchor.Read<SettlementAnchorLocation>().Position + RAID_SPAWN_OFFSET;
                FrontierEncounterSpawner.SpawnEncounter(
                    anchor.Read<CampFlowProgression>().AnchorId,
                    FrontierEncounterKind.Raid,
                    raidSchedule.RaidIdValue,
                    raid.EncounterProfileId,
                    origin);

                ref var mutableSchedule = ref ReplicationMut.Mut<RaidScheduleState>(anchor);
                mutableSchedule.Status = RaidScheduleStatus.Active;

                ref var mutableThreat = ref ReplicationMut.Mut<ThreatState>(anchor);
                mutableThreat.Phase = ThreatPhase.RaidActive;
                mutableThreat.ThreatValue = (ushort)raid.ThreatValue;
            }
        }
    }
}
