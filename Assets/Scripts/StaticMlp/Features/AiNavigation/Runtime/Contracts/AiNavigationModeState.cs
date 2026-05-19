using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct AiNavigationModeState : IComponent
    {
        public AiNavigationMode CurrentMode;
        public AiNavigationMode DesiredMode;
    }
}
