using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientCombatPresentationConfigInitSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new CombatPresentationConfig());
        }
    }
}
