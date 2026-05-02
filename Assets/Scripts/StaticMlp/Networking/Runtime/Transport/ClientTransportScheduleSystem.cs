using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientTransportScheduleSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<UtpTransportContext>();
            ctx.TransportJobHandle = ctx.Driver.ScheduleUpdate();
        }
    }
}
