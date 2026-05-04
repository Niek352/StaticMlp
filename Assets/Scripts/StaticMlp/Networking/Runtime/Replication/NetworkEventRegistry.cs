using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public static class NetworkEventRegistry
    {
        public delegate byte[] Writer<TEvent>(in TEvent evt);
        public delegate bool Reader<TEvent>(byte[] payload, out TEvent evt);

        private static readonly Dictionary<Type, IHandler> HandlersByType = new();
        private static readonly Dictionary<ushort, IHandler> HandlersById = new();

        public static void Clear()
        {
            HandlersByType.Clear();
            HandlersById.Clear();
        }

        public static void Register<TEvent>(
            ushort eventTypeId,
            NetDelivery delivery,
            Writer<TEvent> writer,
            Reader<TEvent> reader)
            where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);
            if (HandlersById.TryGetValue(eventTypeId, out var existingById)
                && existingById.EventType != eventType)
                throw new InvalidOperationException(
                    $"Network event id {eventTypeId} is already registered for {existingById.EventType.FullName}.");

            var handler = new Handler<TEvent>(eventTypeId, delivery, writer, reader);
            HandlersByType[eventType] = handler;
            HandlersById[eventTypeId] = handler;
        }

        public static void RegisterClientWorldTypes()
        {
            if (!CW.Handle.TryGetEventsHandle(typeof(NetworkEventPacket), out _))
                CW.Types().Event<NetworkEventPacket>();
        }

        public static void RegisterServerWorldTypes()
        {
            foreach (var handler in HandlersByType.Values)
                handler.RegisterServerWorldType();
        }

        internal static bool TryCreatePacket<TEvent>(
            NetworkPeerId targetPeer,
            in TEvent evt,
            out NetworkEventPacket packet)
            where TEvent : struct, IEvent
        {
            if (!HandlersByType.TryGetValue(typeof(TEvent), out var handler) || handler is not Handler<TEvent> typedHandler)
            {
                packet = default;
                return false;
            }

            packet = new NetworkEventPacket(
                NetworkRuntime.LocalPeerId,
                targetPeer,
                handler.EventTypeId,
                handler.Delivery,
                typedHandler.Write(evt));
            return true;
        }

        internal static bool TryApplyToServer(in NetworkEventPacket packet)
        {
            return HandlersById.TryGetValue(packet.EventTypeId, out var handler)
                   && handler.TryApplyToServer(in packet);
        }

        private interface IHandler
        {
            Type EventType { get; }
            ushort EventTypeId { get; }
            NetDelivery Delivery { get; }
            void RegisterServerWorldType();
            bool TryApplyToServer(in NetworkEventPacket packet);
        }

        private sealed class Handler<TEvent> : IHandler where TEvent : struct, IEvent
        {
            private readonly Writer<TEvent> _writer;
            private readonly Reader<TEvent> _reader;

            public Handler(
                ushort eventTypeId,
                NetDelivery delivery,
                Writer<TEvent> writer,
                Reader<TEvent> reader)
            {
                EventType = typeof(TEvent);
                EventTypeId = eventTypeId;
                Delivery = delivery;
                _writer = writer ?? throw new ArgumentNullException(nameof(writer));
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            }

            public Type EventType { get; }
            public ushort EventTypeId { get; }
            public NetDelivery Delivery { get; }

            public byte[] Write(TEvent evt) => 
                _writer(in evt);

            public void RegisterServerWorldType()
            {
                SW.Types().Event<NetworkEventFromClient<TEvent>>();
            }

            public bool TryApplyToServer(in NetworkEventPacket packet)
            {
                return _reader(packet.Payload, out var typed)
                       && SW.SendEvent(new NetworkEventFromClient<TEvent>(packet.SourcePeer, in typed));
            }
        }
    }
}
