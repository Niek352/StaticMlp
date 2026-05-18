namespace StaticMlp.Features.Npc
{
    public readonly struct NpcRosterRecordSpawnSpec
    {
        public readonly ushort DefinitionId;
        public readonly NpcClass Class;
        public readonly NpcAcquisitionPath AcquisitionPath;
        public readonly NpcRosterState State;
        public readonly uint CreatedServerTick;

        public NpcRosterRecordSpawnSpec(
            ushort definitionId,
            NpcClass npcClass,
            NpcAcquisitionPath acquisitionPath,
            NpcRosterState state,
            uint createdServerTick)
        {
            DefinitionId = definitionId;
            Class = npcClass;
            AcquisitionPath = acquisitionPath;
            State = state;
            CreatedServerTick = createdServerTick;
        }
    }
}
