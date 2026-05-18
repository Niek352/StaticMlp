using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Npc
{
    [NetworkEntityManifest(typeof(NpcRosterRecord))]
    public struct NpcRosterRecordNetworkEntity : INetworkEntityType
    {
        public byte Id() => 10;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => NpcGameplayFeature.NPC_ROSTER_RECORD;
    }
}
