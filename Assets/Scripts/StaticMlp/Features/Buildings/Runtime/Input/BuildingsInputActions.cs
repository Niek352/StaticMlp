using StaticMlp.Game.Input;

namespace StaticMlp.Features.Buildings
{
    public static class BuildingsInputActions
    {
        public static readonly InputActionName BuildMenuToggle = new("BuildMenuToggle");
        public static readonly InputActionName PlacementRotate = new("PlacementRotate");
        public static readonly InputActionName BuildConstruction = new("BuildConstruction");
    }
}
