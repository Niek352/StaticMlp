using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public static class RequestRegistry
    {
        private static readonly Dictionary<Type, IRequestRegistration> RegistrationsByRequestType = new();
        private static readonly List<IRequestSystemRegistration> SystemRegistrations = new();

        public static bool HasRegistrations => SystemRegistrations.Count > 0;
        public static IReadOnlyList<IRequestSystemRegistration> Registrations => SystemRegistrations;

        public static void Clear()
        {
            RegistrationsByRequestType.Clear();
            SystemRegistrations.Clear();
        }

        public static void Register<TRequest, TResult>(
            IRequestHandler<TRequest, TResult> handler,
            IRequestProjector<TRequest, TResult> projector,
            short serverOrder,
            short clientResultOrder = 200)
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            var registration = new RequestRegistration<TRequest, TResult>(
                handler,
                projector,
                serverOrder,
                clientResultOrder);
            RegistrationsByRequestType[typeof(TRequest)] = registration;
            SystemRegistrations.Add(registration);
        }

        public static bool Send<TRequest>(TRequest request)
            where TRequest : struct, IRequest
        {
            if (!RegistrationsByRequestType.TryGetValue(typeof(TRequest), out var registration))
                throw new InvalidOperationException($"Request {typeof(TRequest).FullName} is not registered.");

            return registration.SendBoxed(request);
        }

        public static TResult HandleServer<TRequest, TResult>(NetworkPeerId sourcePeer, in TRequest request)
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            if (!RegistrationsByRequestType.TryGetValue(typeof(TRequest), out var registration)
                || registration is not RequestRegistration<TRequest, TResult> typed)
                throw new InvalidOperationException(
                    $"Missing request handler registration for {typeof(TRequest).FullName} -> {typeof(TResult).FullName}.");

            return typed.Handle(sourcePeer, in request);
        }

        private interface IRequestRegistration
        {
            bool SendBoxed(object request);
        }

        private sealed class RequestRegistration<TRequest, TResult> : IRequestRegistration, IRequestSystemRegistration
            where TRequest : struct, IRequest<TResult>
            where TResult : struct, IRequestResult
        {
            private readonly IRequestHandler<TRequest, TResult> _handler;
            private readonly IRequestProjector<TRequest, TResult> _projector;

            public RequestRegistration(
                IRequestHandler<TRequest, TResult> handler,
                IRequestProjector<TRequest, TResult> projector,
                short serverOrder,
                short clientResultOrder)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
                _projector = projector;
                ServerOrder = serverOrder;
                ClientResultOrder = clientResultOrder;
            }

            public short ServerOrder { get; }
            public short ClientResultOrder { get; }
            public bool HasProjector => _projector != null;

            public ISystem CreateServerSystem()
            {
                return new ServerRequestHandleSystem<TRequest, TResult>();
            }

            public ISystem CreateClientResultSystem()
            {
                return new ClientRequestResultApplySystem<TRequest, TResult>();
            }

            public TResult Handle(NetworkPeerId sourcePeer, in TRequest request)
            {
                return _handler.Handle(sourcePeer, in request);
            }

            public bool SendBoxed(object request)
            {
                if (request is not TRequest typedRequest)
                    throw new InvalidOperationException($"Invalid request payload type {request?.GetType().FullName}.");

                var pending = CW.GetResource<ClientPendingRequests>();
                typedRequest.RequestId = pending.AllocateId();

                if (!CW.SendToServerEvent(in typedRequest))
                    return false;

                pending.Add(in typedRequest, _projector);
                return true;
            }
        }
    }
}
