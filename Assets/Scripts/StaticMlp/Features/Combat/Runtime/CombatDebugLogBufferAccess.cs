using System;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public static class CombatDebugLogBufferAccess
    {
        public static CombatDebugLogBuffer GetOrCreate()
        {
            if (!SW.HasResource<CombatDebugLogBuffer>())
            {
                SW.SetResource(new CombatDebugLogBuffer());
                return SW.GetResource<CombatDebugLogBuffer>();
            }

            var buffer = SW.GetResource<CombatDebugLogBuffer>();
            if (buffer == null)
                throw new InvalidOperationException("Combat debug log buffer resource exists but is null.");

            return buffer;
        }
    }
}
