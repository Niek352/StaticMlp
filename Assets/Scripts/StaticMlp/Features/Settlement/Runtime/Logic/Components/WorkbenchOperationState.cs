using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct WorkbenchOperationState : IComponent
    {
        public ushort ActiveRecipeId;
        public bool Enabled;
        public byte WorkerSlotCount;
        public float WorkDone;

        public int InputWood;
        public int InputStone;
        public int InputPlanks;
        public int InputSimpleParts;
        public int InputFuel;

        public int OutputPlanks;
        public int OutputSimpleParts;
        public int OutputRepairKits;
        public int OutputMedicine;

        public WorkbenchRecipeId ActiveRecipe => new(ActiveRecipeId);
    }
}
