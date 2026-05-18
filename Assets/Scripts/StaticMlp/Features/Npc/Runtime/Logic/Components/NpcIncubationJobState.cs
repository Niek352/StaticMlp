using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public struct NpcIncubationJobState : IComponent
    {
        public ushort RecipeId;
        public NpcRosterState State;
        public uint StartedAtServerTick;
        public uint CompletesAtServerTick;

        public NpcIncubationRecipeId Recipe => new(RecipeId);
        public bool IsComplete(uint currentServerTick) => currentServerTick >= CompletesAtServerTick;
    }
}
