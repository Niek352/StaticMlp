namespace StaticMlp.Networking.Requests
{
    public static class RequestApi
    {
        public static void Send<TRequest, TResult>(TRequest request)
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            RequestRegistry.Send<TRequest, TResult>(request);
        }
    }
}
