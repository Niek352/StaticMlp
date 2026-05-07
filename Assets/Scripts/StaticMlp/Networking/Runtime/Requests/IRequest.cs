using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public interface IRequest : IEvent
    {
        RequestId RequestId { get; set; }
    }

    public interface IRequest<TResult> : IRequest
        where TResult : struct, IRequestResult
    {
    }
}
