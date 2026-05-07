using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public sealed class InitialBotSpawningResource : IResource
    {
        public InitialBotSpawningResource(InitialBotSpawnDefinition[] spawns)
        {
            Spawns = spawns ?? System.Array.Empty<InitialBotSpawnDefinition>();
        }

        public InitialBotSpawnDefinition[] Spawns { get; }
    }
}
