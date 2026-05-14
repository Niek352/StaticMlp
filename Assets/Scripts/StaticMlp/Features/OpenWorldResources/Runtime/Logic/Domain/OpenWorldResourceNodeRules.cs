using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public static class OpenWorldResourceNodeRules
    {
        public static int StartingAmount(ResourcePlacementKindId kindId)
        {
            return kindId.Value == 2 ? 8 : 5;
        }
    }
}
