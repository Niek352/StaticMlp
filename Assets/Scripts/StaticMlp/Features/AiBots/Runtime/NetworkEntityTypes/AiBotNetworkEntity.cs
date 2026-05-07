using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    [NetworkEntityManifest(typeof(CharacterNetState), typeof(AiNetState))]
    public struct AiBotNetworkEntity : INetworkEntityType
    {
        public byte Id() => 5;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => AiBotsGameplayFeature.BOT;
    }
}
