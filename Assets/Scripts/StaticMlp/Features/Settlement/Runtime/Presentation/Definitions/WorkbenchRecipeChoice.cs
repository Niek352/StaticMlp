namespace StaticMlp.Features.Settlement
{
    public readonly struct WorkbenchRecipeChoice
    {
        public readonly ushort RecipeId;
        public readonly bool IsActive;

        public WorkbenchRecipeChoice(ushort recipeId, bool isActive)
        {
            RecipeId = recipeId;
            IsActive = isActive;
        }
    }
}
