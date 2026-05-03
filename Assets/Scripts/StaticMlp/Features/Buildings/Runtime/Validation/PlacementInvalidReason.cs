namespace StaticMlp.Features.Buildings
{
    public enum PlacementInvalidReason : byte
    {
        None = 0,
        UnknownBuilding = 1,
        OffGround = 2,
        SlopeTooSteep = 3,
        Occupied = 4,
        RestrictedZone = 5
    }
}
