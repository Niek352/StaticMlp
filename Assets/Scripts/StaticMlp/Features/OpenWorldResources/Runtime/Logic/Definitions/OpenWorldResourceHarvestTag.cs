using System;

namespace StaticMlp.Features.OpenWorldResources
{
    [Flags]
    public enum OpenWorldResourceHarvestTag : byte
    {
        None = 0,
        Axe = 1 << 0,
        Blunt = 1 << 1,
        Lightning = 1 << 2,
        Fire = 1 << 3,
        Rune = 1 << 4,
        Projectile = 1 << 5
    }
}
