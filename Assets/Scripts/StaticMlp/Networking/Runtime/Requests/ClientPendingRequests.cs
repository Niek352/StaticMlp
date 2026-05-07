using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public sealed class ClientPendingRequests : IResource
    {
        private readonly Dictionary<RequestId, IPendingRequest> _entries = new();
        private uint _nextRequestValue = 1;

        public RequestId AllocateId()
        {
            var next = _nextRequestValue++;
            if (next == 0)
                next = _nextRequestValue++;

            return new RequestId(next);
        }

        public void Add<TRequest, TResult>(in TRequest request, IRequestProjector<TRequest, TResult> projector)
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            _entries[request.RequestId] = new PendingRequest<TRequest, TResult>(request, projector);
        }

        public void ProjectAll()
        {
            foreach (var entry in _entries.Values)
                entry.Project();
        }

        public bool TryResolve<TRequest, TResult>(in TResult result)
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            if (!_entries.TryGetValue(result.RequestId, out var entry))
                return false;

            if (entry is not PendingRequest<TRequest, TResult> typedEntry)
                throw new InvalidOperationException(
                    $"Pending request {result.RequestId.Value} does not match {typeof(TRequest).FullName} -> {typeof(TResult).FullName}.");

            _entries.Remove(result.RequestId);
            typedEntry.OnResolved(in result);
            return true;
        }

        private interface IPendingRequest
        {
            void Project();
        }

        private sealed class PendingRequest<TRequest, TResult> : IPendingRequest
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            private readonly TRequest _request;
            private readonly IRequestProjector<TRequest, TResult> _projector;

            public PendingRequest(TRequest request, IRequestProjector<TRequest, TResult> projector)
            {
                _request = request;
                _projector = projector;
            }

            public void Project()
            {
                _projector?.Project(in _request);
            }

            public void OnResolved(in TResult result)
            {
                _projector?.OnResolved(in _request, in result);
            }
        }
    }
}
