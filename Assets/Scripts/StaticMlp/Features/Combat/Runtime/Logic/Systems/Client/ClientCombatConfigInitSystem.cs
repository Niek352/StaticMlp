using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientCombatConfigInitSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new CombatConfig());
        }
    }
}
