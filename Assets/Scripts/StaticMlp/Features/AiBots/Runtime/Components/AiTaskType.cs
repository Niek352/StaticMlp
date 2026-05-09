namespace StaticMlp.Features.AiBots
{
    public enum AiTaskType : ushort
    {
        Idle = 0,
        GatherWood = 1,
        Eat = 2,
        AttackEnemy = 3,
        Flee = 4,
        FollowLeader = 5,
        BuildConstruction = 6,
        DeliveryResourceToBuilding = 7
    }
}
