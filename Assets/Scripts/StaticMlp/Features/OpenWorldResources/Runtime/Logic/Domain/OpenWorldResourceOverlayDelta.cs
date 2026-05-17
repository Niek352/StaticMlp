using System;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceOverlayDelta : IEquatable<OpenWorldResourceOverlayDelta>
    {
        public long PlacementId;
        public ushort RemainingAmount;
        public OpenWorldResourceOverlayFlags Flags;
        public uint RespawnTick;

        public bool Equals(OpenWorldResourceOverlayDelta other)
        {
            return PlacementId == other.PlacementId
                   && RemainingAmount == other.RemainingAmount
                   && Flags == other.Flags
                   && RespawnTick == other.RespawnTick;
        }

        public override bool Equals(object obj)
        {
            return obj is OpenWorldResourceOverlayDelta other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = PlacementId.GetHashCode();
                hash = (hash * 397) ^ RemainingAmount.GetHashCode();
                hash = (hash * 397) ^ (int)Flags;
                hash = (hash * 397) ^ RespawnTick.GetHashCode();
                return hash;
            }
        }
    }
}
