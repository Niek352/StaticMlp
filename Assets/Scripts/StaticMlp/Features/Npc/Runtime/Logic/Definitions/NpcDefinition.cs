namespace StaticMlp.Features.Npc
{
    public readonly struct NpcDefinition
    {
        public readonly NpcDefinitionId Id;
        public readonly NpcClass Class;
        public readonly NpcRoleFlags Roles;
        public readonly NpcAcquisitionPathFlags AllowedAcquisitionPaths;

        public NpcDefinition(
            NpcDefinitionId id,
            NpcClass npcClass,
            NpcRoleFlags roles,
            NpcAcquisitionPathFlags allowedAcquisitionPaths)
        {
            Id = id;
            Class = npcClass;
            Roles = roles;
            AllowedAcquisitionPaths = allowedAcquisitionPaths;
        }
    }
}
