using System;
using System.Collections.Generic;

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

        public static bool TryWrite<TEvent>(
            in TEvent evt,
            out ushort eventTypeId,
            out byte[] payload,
            out NetDelivery delivery)
        {
            if (!HandlersByType.TryGetValue(typeof(TEvent), out var handler))
            {
                eventTypeId = default;
                payload = default;
                delivery = default;
                return false;
            }

            eventTypeId = handler.EventTypeId;
            payload = handler.WriteBoxed(evt);
            delivery = handler.Delivery;
            return true;
        }

        public static bool TryRead<TEvent>(NetworkEventMessage message, out TEvent evt)
        {
            if (message == null
                || !HandlersById.TryGetValue(message.EventTypeId, out var handler)
                || handler.EventType != typeof(TEvent)
                || !handler.TryRead(message.Payload, out var boxed))
            {
                evt = default;
                return false;
            }

            evt = (TEvent)boxed;
            return true;
        }

        private interface IHandler
        {
            Type EventType { get; }
            ushort EventTypeId { get; }
            NetDelivery Delivery { get; }
            byte[] WriteBoxed(object evt);
            bool TryRead(byte[] payload, out object evt);
        }

        private sealed class Handler<TEvent> : IHandler
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

            public byte[] WriteBoxed(object evt)
            {
                var typed = (TEvent)evt;
                return _writer(in typed);
            }

            public bool TryRead(byte[] payload, out object evt)
            {
                if (_reader(payload, out var typed))
                {
                    evt = typed;
                    return true;
                }

                evt = default;
                return false;
            }
        }
    }
}
