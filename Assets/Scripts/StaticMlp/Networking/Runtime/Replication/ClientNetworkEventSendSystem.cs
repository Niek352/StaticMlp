using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Networking.Replication
{
    public sealed class ClientNetworkEventSendSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, NetworkEventPacket> _events;

        public void Init()
        {
            _events = CW.RegisterEventReceiver<NetworkEventPacket>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _events);
        }

        public void Update()
        {
            var canSend = NetworkEvents.ClientCanSendToServer;

            NetOutbox outbox = null;
            if (canSend)
                outbox = CW.GetResource<NetOutbox>();
            
            var i = 0;
            foreach (var evt in _events)
            {
                i++;
                if (!canSend)
                    continue;
               
                var packet = evt.Value;
                outbox.EnqueueNetworkEvent(in packet);
            }
            
            Debug.Log($"[ClientNetworkEventSend] canSend={canSend} LocalPeerId={NetworkRuntime.LocalPeerId.Value} eventCount={i}");
        }
    }
}
