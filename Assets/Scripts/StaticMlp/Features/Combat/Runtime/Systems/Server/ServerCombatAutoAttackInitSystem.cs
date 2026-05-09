using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerCombatAutoAttackInitSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(new CombatAutoAttackConfig());
        }
    }
}
