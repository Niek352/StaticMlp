using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerTransportScheduleSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();
            ctx.TransportJobHandle = ctx.Driver.ScheduleUpdate();
        }
    }
}
