using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public struct ExtractableState : IComponent
    {
        public ushort NpcDefinitionId;
        public uint ExpiresAtServerTick;

        public NpcDefinitionId Definition => new(NpcDefinitionId);
        public bool IsExpired(uint currentServerTick) => currentServerTick > ExpiresAtServerTick;
    }
}
