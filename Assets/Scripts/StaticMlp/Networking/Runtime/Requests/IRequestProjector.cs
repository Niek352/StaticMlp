namespace StaticMlp.Networking.Requests
{
    public interface IRequestProjector<TRequest, TResult>
        where TRequest : struct, IRequest<TResult>
        where TResult : struct, IRequestResult
    {
        void Project(in TRequest request);
        void OnResolved(in TRequest request, in TResult result);
    }
}
