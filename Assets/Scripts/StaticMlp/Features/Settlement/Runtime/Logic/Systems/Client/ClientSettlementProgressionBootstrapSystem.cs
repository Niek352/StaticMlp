using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementProgressionBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new SettlementProgressionState { SettlementLevel = 1 });
        }
    }
}
