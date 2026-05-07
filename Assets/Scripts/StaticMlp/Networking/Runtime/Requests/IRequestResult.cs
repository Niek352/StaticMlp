using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public interface IRequestResult : IEvent
    {
        RequestId RequestId { get; }
        RequestStatus Status { get; set; }
    }
}
