using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerCombatConfigInitSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(new CombatConfig());
        }
    }
}
