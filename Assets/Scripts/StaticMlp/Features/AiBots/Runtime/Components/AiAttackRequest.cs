using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public struct AiAttackRequest : IComponent
    {
        public EntityGID Target;
    }
}
