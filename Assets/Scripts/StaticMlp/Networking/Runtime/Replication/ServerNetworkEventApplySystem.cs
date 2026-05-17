using FFS.Libraries.StaticEcs;
using UnityEngine;

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
                var applied = NetworkEventRegistry.TryApplyToServer(in packet);
            }
        }
    }
}
