using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct LocalNavAttachState : IComponent
    {
        public bool IsReady;
        public int ZoneId;
        public int NavVersion;
    }
}
