namespace StaticMlp.Features.Buildings
{
    public readonly struct PlacementValidationResult
    {
        public readonly bool IsValid;
        public readonly PlacementInvalidReason Reason;

        private PlacementValidationResult(bool isValid, PlacementInvalidReason reason)
        {
            IsValid = isValid;
            Reason = reason;
        }

        public static PlacementValidationResult Valid() => new(true, PlacementInvalidReason.None);
        public static PlacementValidationResult Invalid(PlacementInvalidReason reason) => new(false, reason);
    }
}
