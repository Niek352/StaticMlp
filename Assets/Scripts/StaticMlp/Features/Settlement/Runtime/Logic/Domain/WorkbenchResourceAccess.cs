namespace StaticMlp.Features.Settlement
{
    public static class WorkbenchResourceAccess
    {
        public static int GetInput(in WorkbenchOperationState state, ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.WoodId)
                return state.InputWood;

            if (resourceId == ResourceCatalog.StoneId)
                return state.InputStone;

            if (resourceId == ResourceCatalog.PlanksId)
                return state.InputPlanks;

            if (resourceId == ResourceCatalog.SimplePartsId)
                return state.InputSimpleParts;

            if (resourceId == ResourceCatalog.FuelId)
                return state.InputFuel;

            return 0;
        }

        public static int GetOutput(in WorkbenchOperationState state, ResourceId resourceId)
        {
            if (resourceId == ResourceCatalog.PlanksId)
                return state.OutputPlanks;

            if (resourceId == ResourceCatalog.SimplePartsId)
                return state.OutputSimpleParts;

            if (resourceId == ResourceCatalog.RepairKitsId)
                return state.OutputRepairKits;

            if (resourceId == ResourceCatalog.MedicineId)
                return state.OutputMedicine;

            return 0;
        }
    }
}
