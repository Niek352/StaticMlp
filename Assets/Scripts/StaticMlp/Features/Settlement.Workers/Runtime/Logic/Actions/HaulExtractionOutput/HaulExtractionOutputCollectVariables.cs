using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class HaulExtractionOutputCollectVariables : IAiActionVariableCollector
    {
        public const ushort TargetExtractionBuilding = 2203;

        public void Collect(SW.Entity entity)
        {
            var target = FindNearestExtractionOutput(entity);
            if (target.TryUnpack<ServerWT>(out _))
                AiBlackboardAccess.SetEntity(entity, TargetExtractionBuilding, target);
            else
                AiBlackboardAccess.Remove(entity, TargetExtractionBuilding);
        }

        private static EntityGID FindNearestExtractionOutput(SW.Entity worker)
        {
            ref readonly var workerState = ref worker.Read<CharacterNetState>();
            var bestDistanceSq = float.MaxValue;
            var bestBuilding = default(EntityGID);

            foreach (var building in SW.Query<All<FinishedBuildingTag, ExtractionOperationState, ConstructionTransform>>().Entities())
            {
                ref readonly var operation = ref building.Read<ExtractionOperationState>();
                if (!ExtractionRules.HasOutput(in operation))
                    continue;

                ref readonly var transform = ref building.Read<ConstructionTransform>();
                var distanceSq = (transform.Position - workerState.Position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestBuilding = building.GID;
            }

            return bestBuilding;
        }
    }
}
