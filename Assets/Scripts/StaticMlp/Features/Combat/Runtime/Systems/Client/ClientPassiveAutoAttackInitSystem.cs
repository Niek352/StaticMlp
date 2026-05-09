using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackInitSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new CombatAutoAttackConfig());
        }
    }
}
