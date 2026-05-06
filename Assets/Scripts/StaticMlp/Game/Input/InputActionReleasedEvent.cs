using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game.Input
{
    public readonly struct InputActionReleasedEvent : IEvent, IEventConfig<InputActionReleasedEvent>
    {
        public readonly InputActionName Action;

        public InputActionReleasedEvent(InputActionName action)
        {
            Action = action;
        }

        public EventTypeConfig<InputActionReleasedEvent> Config()
        {
            return new EventTypeConfig<InputActionReleasedEvent>(
                guid: new Guid("51c3b5fd-fc92-4e07-9f3d-87d56666d845"));
        }
    }
}
