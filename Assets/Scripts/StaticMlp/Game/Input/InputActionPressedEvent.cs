using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game.Input
{
    public readonly struct InputActionPressedEvent : IEvent, IEventConfig<InputActionPressedEvent>
    {
        public readonly InputActionName Action;

        public InputActionPressedEvent(InputActionName action)
        {
            Action = action;
        }

        public EventTypeConfig<InputActionPressedEvent> Config()
        {
            return new EventTypeConfig<InputActionPressedEvent>(
                guid: new Guid("2f724a92-c50a-49e6-a020-b69eea5148e7"));
        }
    }
}
