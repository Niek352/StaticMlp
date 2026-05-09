using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public sealed class ServerNetworkEventApplySystem : ISystem
    {
        public void Update()
        {
            ref var inbox = ref SW.GetResource<NetInbox>();
            for (var i = 0; i < inbox.Events.Count; i++)
            {
                var packet = inbox.Events[i];
                NetworkEventRegistry.TryApplyToServer(in packet);
            }
        }
    }
}
