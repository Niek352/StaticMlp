using System;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceOverlayState : IEquatable<OpenWorldResourceOverlayState>
    {
        public long PlacementId;
        public ushort KindIdValue;
        public ushort RemainingAmount;
        public OpenWorldResourceOverlayFlags Flags;
        public uint RespawnTick;

        public bool Equals(OpenWorldResourceOverlayState other)
        {
            return PlacementId == other.PlacementId
                   && KindIdValue == other.KindIdValue
                   && RemainingAmount == other.RemainingAmount
                   && Flags == other.Flags
                   && RespawnTick == other.RespawnTick;
        }

        public override bool Equals(object obj)
        {
            return obj is OpenWorldResourceOverlayState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = PlacementId.GetHashCode();
                hash = (hash * 397) ^ KindIdValue.GetHashCode();
                hash = (hash * 397) ^ RemainingAmount.GetHashCode();
                hash = (hash * 397) ^ (int)Flags;
                hash = (hash * 397) ^ RespawnTick.GetHashCode();
                return hash;
            }
        }
    }
}
