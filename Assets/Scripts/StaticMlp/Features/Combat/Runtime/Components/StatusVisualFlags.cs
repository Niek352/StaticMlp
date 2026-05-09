using System;

namespace StaticMlp.Features.Combat
{
    [Flags]
    public enum StatusVisualFlags : byte
    {
        None = 0,
        Poison = 1 << 0,
        Burning = 1 << 1,
        Oiled = 1 << 2,
    }
}
