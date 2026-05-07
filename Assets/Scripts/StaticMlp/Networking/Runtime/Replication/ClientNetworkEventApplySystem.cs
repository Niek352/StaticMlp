using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public sealed class ClientNetworkEventApplySystem : ISystem
    {
        public void Update()
        {
            if (!CW.HasResource<NetInbox>())
                return;

            ref var inbox = ref CW.GetResource<NetInbox>();
            for (var i = 0; i < inbox.Events.Count; i++)
            {
                var packet = inbox.Events[i];
                NetworkEventRegistry.TryApplyToClient(in packet);
            }
        }
    }
}
