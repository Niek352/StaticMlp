using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct WorkbenchOperationState : IComponent
    {
        public ushort ActiveRecipeId;
        public bool Enabled;
        public byte WorkerSlotCount;
        public float WorkDone;

        public WorkbenchRecipeId ActiveRecipe => new(ActiveRecipeId);
    }
}
