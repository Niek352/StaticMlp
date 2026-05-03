using System;

namespace StaticMlp.Networking.Replication
{
    public static class NetworkEvents
    {
        public delegate void ServerHandler<TEvent>(NetworkPeerId sourcePeer, in TEvent evt);

        public static readonly NetworkPeerId ServerPeer = new(0);

        public static bool ClientCanSendToServer =>
            NetworkRuntime.LocalPeerId.Value != 0
            && CW.IsWorldInitialized
            && CW.HasResource<NetOutbox>();

        public static bool TrySendToServer<TEvent>(in TEvent evt)
        {
            if (!ClientCanSendToServer
                || !NetworkEventRegistry.TryWrite(in evt, out var eventTypeId, out var payload, out var delivery))
                return false;

            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.EnqueueNetworkEvent(ServerPeer, eventTypeId, payload, delivery);
            return true;
        }

        public static void SendToServer<TEvent>(in TEvent evt)
        {
            if (!TrySendToServer(in evt))
                throw new InvalidOperationException($"Network event {typeof(TEvent).FullName} is not registered or the client is not connected.");
        }

        public static void ForEachServer<TEvent>(ServerHandler<TEvent> handler)
        {
            if (handler == null
                || !SW.IsWorldInitialized
                || !SW.HasResource<NetInbox>())
                return;

            ref var inbox = ref SW.GetResource<NetInbox>();
            for (var i = 0; i < inbox.Events.Count; i++)
            {
                var message = inbox.Events[i];
                if (NetworkEventRegistry.TryRead<TEvent>(message, out var evt))
                    handler(message.SourcePeer, in evt);
            }
        }
    }
}
