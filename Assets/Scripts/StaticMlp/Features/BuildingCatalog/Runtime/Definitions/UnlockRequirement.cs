namespace StaticMlp.Features.BuildingCatalog
{
    public readonly struct UnlockRequirement
    {
        public readonly UnlockRequirementKind Kind;
        public readonly int IntParameter;

        public UnlockRequirement(UnlockRequirementKind kind, int intParameter = 0)
        {
            Kind = kind;
            IntParameter = intParameter;
        }

        public static UnlockRequirement None => new(UnlockRequirementKind.None);

        public static UnlockRequirement SettlementLevel(int level)
        {
            return new UnlockRequirement(UnlockRequirementKind.SettlementLevel, level);
        }

        public static UnlockRequirement BuildingConstructed(BuildingId buildingId)
        {
            return new UnlockRequirement(UnlockRequirementKind.BuildingConstructed, buildingId.Value);
        }
    }
}
