namespace StaticMlp.Features.Settlement
{
    public interface ISettlementUnlockReadModel
    {
        int SettlementLevel { get; }

        bool HasConstructedBuilding(int buildingIdValue);
    }
}
