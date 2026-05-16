using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public static class NetworkEventRegistry {
        public delegate void Writer<TEvent>(ref NetworkWriter writer, in TEvent evt);
        public delegate TEvent Reader<TEvent>(ref NetworkReader reader);

        [Obsolete("Temp")]
        public delegate byte[] LegacyWriter<TEvent>(in TEvent evt);

        [Obsolete("Temp")]
        public delegate bool LegacyReader<TEvent>(byte[] payload, out TEvent evt);

        private static readonly Dictionary<Type, IHandler> HandlersByType = new();
        private static readonly Dictionary<ushort, IHandler> HandlersById = new();

        public static void Clear() {
            HandlersByType.Clear();
            HandlersById.Clear();
        }

        public static void Register<TEvent>(
            ushort eventTypeId,
            NetDelivery delivery,
            int byteSizeHint,
            Writer<TEvent> writer,
            Reader<TEvent> reader)
            where TEvent : struct, IEvent {
            RegisterHandler(new Handler<TEvent>(eventTypeId, delivery, byteSizeHint, writer, reader));
        }

        [Obsolete("Temp")]
        public static void Register<TEvent>(
            ushort eventTypeId,
            NetDelivery delivery,
            LegacyWriter<TEvent> writer,
            LegacyReader<TEvent> reader)
            where TEvent : struct, IEvent {
            RegisterHandler(new LegacyHandler<TEvent>(eventTypeId, delivery, writer, reader));
        }

        public static void RegisterClientWorldTypes() {
            if (!CW.Handle.TryGetEventsHandle(typeof(NetworkEventPacket), out _))
                CW.Types().Event<NetworkEventPacket>();

            foreach (var handler in HandlersByType.Values)
                handler.RegisterClientWorldType();
        }

        public static void RegisterServerWorldTypes() {
            foreach (var handler in HandlersByType.Values)
                handler.RegisterServerWorldType();
        }

        public static string GetEventDisplayName(ushort eventTypeId) {
            return HandlersById.TryGetValue(eventTypeId, out var handler)
                ? handler.EventType.Name
                : $"Event({eventTypeId})";
        }

        internal static void CreatePacket<TEvent>(
            NetworkPeerId targetPeer,
            in TEvent evt,
            out NetworkEventPacket packet)
            where TEvent : struct, IEvent {
            if (!HandlersByType.TryGetValue(typeof(TEvent), out var handler) || handler is not IHandler<TEvent> typedHandler)
                throw new ArgumentException($"Network event `{typeof(TEvent).FullName}` is not registered.");

            packet = typedHandler.CreatePacket(NetworkRuntime.LocalPeerId, targetPeer, in evt);
        }

        internal static bool DecodePacket(
            NetworkPeerId sourcePeer,
            ushort eventTypeId,
            byte[] payload,
            out NetworkEventPacket packet) {
            if (!HandlersById.TryGetValue(eventTypeId, out var handler)) {
                packet = default;
                return false;
            }

            return handler.TryDecode(sourcePeer, eventTypeId, payload, out packet);
        }

        internal static bool TryApplyToServer(in NetworkEventPacket packet) {
            var hasHandler = HandlersById.TryGetValue(packet.EventTypeId, out var handler);
            var applied = hasHandler && packet.Payload.TryApplyToServer(packet.SourcePeer);
            Debug.Log($"[NetworkEventRegistry] TryApplyToServer eventId={packet.EventTypeId} handler={handler?.EventType.Name ?? "null"} applied={applied}");
            return applied;
        }

        internal static bool TryApplyToClient(in NetworkEventPacket packet) {
            var hasHandler = HandlersById.TryGetValue(packet.EventTypeId, out var handler);
            var applied = hasHandler && packet.Payload.TryApplyToClient(packet.SourcePeer);
            Debug.Log($"[NetworkEventRegistry] TryApplyToClient eventId={packet.EventTypeId} handler={handler?.EventType.Name ?? "null"} applied={applied}");
            return applied;
        }

        private static void RegisterHandler(IHandler handler) {
            if (HandlersById.TryGetValue(handler.EventTypeId, out var existingById)
                && existingById.EventType != handler.EventType) {
                throw new InvalidOperationException(
                    $"Network event id {handler.EventTypeId} is already registered for {existingById.EventType.FullName}.");
            }

            HandlersByType[handler.EventType] = handler;
            HandlersById[handler.EventTypeId] = handler;
        }

        private interface IHandler {
            Type EventType { get; }
            ushort EventTypeId { get; }
            void RegisterClientWorldType();
            void RegisterServerWorldType();
            bool TryDecode(NetworkPeerId sourcePeer, ushort eventTypeId, byte[] payload, out NetworkEventPacket packet);
        }

        private interface IHandler<TEvent> where TEvent : struct, IEvent {
            NetworkEventPacket CreatePacket(NetworkPeerId sourcePeer, NetworkPeerId targetPeer, in TEvent evt);
        }

        private sealed class Handler<TEvent> : IHandler, IHandler<TEvent>
            where TEvent : struct, IEvent {
            private readonly Writer<TEvent> _writer;
            private readonly Reader<TEvent> _reader;
            private readonly int _byteSizeHint;
            private readonly NetDelivery _delivery;

            public Handler(
                ushort eventTypeId,
                NetDelivery delivery,
                int byteSizeHint,
                Writer<TEvent> writer,
                Reader<TEvent> reader) {
                EventType = typeof(TEvent);
                EventTypeId = eventTypeId;
                _delivery = delivery;
                _byteSizeHint = byteSizeHint;
                _writer = writer ?? throw new ArgumentNullException(nameof(writer));
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            }

            public Type EventType { get; }
            public ushort EventTypeId { get; }

            public NetworkEventPacket CreatePacket(NetworkPeerId sourcePeer, NetworkPeerId targetPeer, in TEvent evt) {
                return new NetworkEventPacket(
                    sourcePeer,
                    targetPeer,
                    EventTypeId,
                    _delivery,
                    _byteSizeHint,
                    new NetworkEventPayload<TEvent>(_writer, in evt));
            }

            public bool TryDecode(NetworkPeerId sourcePeer, ushort eventTypeId, byte[] payload, out NetworkEventPacket packet) {
                var reader = new NetworkReader(payload);
                var evt = _reader(ref reader);
                packet = new NetworkEventPacket(
                    sourcePeer,
                    NetworkRuntime.LocalPeerId,
                    eventTypeId,
                    _delivery,
                    payload.Length,
                    new NetworkEventPayload<TEvent>(_writer, in evt));
                return true;
            }

            public void RegisterClientWorldType() {
                CW.Types().Event<NetworkEventFromServer<TEvent>>();
            }

            public void RegisterServerWorldType() {
                SW.Types().Event<NetworkEventFromClient<TEvent>>();
            }
        }

        [Obsolete("Temp")]
        private sealed class LegacyHandler<TEvent> : IHandler, IHandler<TEvent>
            where TEvent : struct, IEvent {
            private readonly LegacyWriter<TEvent> _writer;
            private readonly LegacyReader<TEvent> _reader;
            private readonly NetDelivery _delivery;

            public LegacyHandler(
                ushort eventTypeId,
                NetDelivery delivery,
                LegacyWriter<TEvent> writer,
                LegacyReader<TEvent> reader) {
                EventType = typeof(TEvent);
                EventTypeId = eventTypeId;
                _delivery = delivery;
                _writer = writer ?? throw new ArgumentNullException(nameof(writer));
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            }

            public Type EventType { get; }
            public ushort EventTypeId { get; }

            public NetworkEventPacket CreatePacket(NetworkPeerId sourcePeer, NetworkPeerId targetPeer, in TEvent evt) {
                return new NetworkEventPacket(
                    sourcePeer,
                    targetPeer,
                    EventTypeId,
                    _delivery,
                    64,
                    new LegacyNetworkEventPayload<TEvent>(_writer, in evt));
            }

            public bool TryDecode(NetworkPeerId sourcePeer, ushort eventTypeId, byte[] payload, out NetworkEventPacket packet) {
                if (!_reader(payload, out var evt)) {
                    packet = default;
                    return false;
                }

                packet = new NetworkEventPacket(
                    sourcePeer,
                    NetworkRuntime.LocalPeerId,
                    eventTypeId,
                    _delivery,
                    payload.Length,
                    new LegacyNetworkEventPayload<TEvent>(_writer, in evt));
                return true;
            }

            public void RegisterClientWorldType() {
                CW.Types().Event<NetworkEventFromServer<TEvent>>();
            }

            public void RegisterServerWorldType() {
                SW.Types().Event<NetworkEventFromClient<TEvent>>();
            }
        }
    }
}
