using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking.Requests
{
    public sealed class ClientRequestResultApplySystem<TRequest, TResult> : ISystem
        where TRequest : struct, IRequest<TResult>
        where TResult : struct, IRequestResult
    {
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<TResult>> _results;

        public void Init()
        {
            _results = CW.RegisterEventReceiver<NetworkEventFromServer<TResult>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _results);
        }

        public void Update()
        {
            var pending = CW.GetResource<ClientPendingRequests>();
            foreach (var evt in _results)
                pending.TryResolve<TRequest, TResult>(in evt.Value.Value);
        }
    }
}
