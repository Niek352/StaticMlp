namespace StaticMlp.Networking.Requests
{
    public interface IRequestHandler<TRequest, TResult>
        where TRequest : struct, IRequest<TResult>
        where TResult : struct, IRequestResult
    {
        TResult Handle(NetworkPeerId sourcePeer, in TRequest request);
    }
}
