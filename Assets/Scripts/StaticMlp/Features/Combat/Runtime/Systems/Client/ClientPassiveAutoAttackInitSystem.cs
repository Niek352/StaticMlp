using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackInitSystem : ISystem
    {
        public void Init()
        {
            if (!CW.HasResource<CombatAutoAttackConfig>())
            {
                CW.SetResource(new CombatAutoAttackConfig());
            }
        }
    }
}
