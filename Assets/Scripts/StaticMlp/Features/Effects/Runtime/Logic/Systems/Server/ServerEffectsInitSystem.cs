using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Effects
{
    public sealed class ServerEffectsInitSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(new CombatDebugLogBuffer());
        }
    }
}
