using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Requests
{
    public sealed class ServerRequestHandleSystem<TRequest, TResult> : ISystem
        where TRequest : struct, IRequest<TResult>
        where TResult : struct, IRequestResult
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<TRequest>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<TRequest>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
            {
                var result = RequestRegistry.HandleServer<TRequest, TResult>(evt.Value.SourcePeer, in evt.Value.Value);
                SW.SendToPeerEvent(evt.Value.SourcePeer, in result);
            }
        }
    }
}
