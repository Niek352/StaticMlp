using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;

namespace StaticMlp.Features.Npc
{
    public sealed class ServerNpcIncubationJobCompleteSystem : ISystem
    {
        private readonly List<EntityGID> _completedJobs = new();

        public void Update()
        {
            _completedJobs.Clear();

            var simulationTime = SW.GetResource<SimulationTime>();
            var currentTick = simulationTime.ServerTick;

            foreach (var entity in SW.Query<All<NpcIncubationJobState>>().Entities())
            {
                ref readonly var job = ref entity.Read<NpcIncubationJobState>();
                if (job.IsComplete(currentTick))
                    _completedJobs.Add(entity.GID);
            }

            var factory = SW.GetResource<NpcRosterRecordFactory>();

            for (var i = 0; i < _completedJobs.Count; i++)
            {
                var gid = _completedJobs[i];
                if (!gid.TryUnpack<ServerWT>(out var entity))
                    continue;

                ref readonly var job = ref entity.Read<NpcIncubationJobState>();
                var recipe = NpcIncubationRecipeCatalog.Get(job.Recipe);
                var npcDefinition = NpcDefinitionCatalog.Get(recipe.ResultNpc);

                var rosterRecord = factory.Spawn(new NpcRosterRecordSpawnSpec(
                    npcDefinition.Id.Value,
                    npcDefinition.Class,
                    NpcAcquisitionPath.Incubation,
                    NpcRosterState.Recruited,
                    currentTick));

                SW.SendEvent(new NpcIncubationJobCompletedEvent(gid, rosterRecord));

                entity.Delete<NpcIncubationJobState>();
            }
        }
    }
}
