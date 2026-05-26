using System;

namespace StaticMlp.Features.OpenWorldResources
{
    [Flags]
    public enum OpenWorldResourceOverlayFlags : byte
    {
        None = 0,
        Depleted = 1 << 0,
        Hidden = 1 << 1,
        Respawning = 1 << 2,
        Replaced = 1 << 3
    }
}
