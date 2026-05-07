using System;

namespace StaticMlp.Networking.Requests
{
    public static class RequestApi
    {
        public static bool Send<TRequest>(TRequest request)
            where TRequest : struct, IRequest
        {
            if (!CW.IsWorldInitialized)
                throw new InvalidOperationException("Client world must be initialized before sending requests.");

            if (!CW.HasResource<ClientPendingRequests>())
                throw new InvalidOperationException("Client pending request resource is not initialized.");

            return RequestRegistry.Send(request);
        }
    }
}
