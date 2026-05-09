using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Features.Combat;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    [NetworkEntityManifest(typeof(CharacterNetState), typeof(AiNetState), typeof(Health))]
    public struct AiBotNetworkEntity : INetworkEntityType
    {
        public byte Id() => 5;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => AiBotsGameplayFeature.BOT;
    }
}
