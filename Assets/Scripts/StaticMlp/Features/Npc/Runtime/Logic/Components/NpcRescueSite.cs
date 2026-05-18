using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public struct NpcRescueSite : IComponent
    {
        public ushort NpcDefinitionId;
        public NpcRescueSiteState State;

        public NpcDefinitionId Definition => new(NpcDefinitionId);
    }
}
