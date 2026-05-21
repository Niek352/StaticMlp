using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
{
    public readonly struct SettlementWorkerDemand
    {
        public enum DemandKind : byte
        {
            None = 0,
            ConstructionDelivery = 1,
            ConstructionBuild = 2,
            Gather = 3,
            Haul = 4,
            Process = 5
        }

        public readonly DemandKind Kind;
        public readonly AiTaskType Task;
        public readonly EntityGID Target;
        public readonly ResourceId Resource;
        public readonly int Amount;

        public SettlementWorkerDemand(
            DemandKind kind,
            AiTaskType task,
            EntityGID target,
            ResourceId resource,
            int amount)
        {
            Kind = kind;
            Task = task;
            Target = target;
            Resource = resource;
            Amount = amount;
        }
    }
}
