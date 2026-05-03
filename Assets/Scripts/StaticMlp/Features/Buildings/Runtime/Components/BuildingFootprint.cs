using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Buildings
{
    public struct BuildingFootprint : IComponent
    {
        public float Width;
        public float Length;

        public BuildingFootprint(float width, float length)
        {
            Width = width;
            Length = length;
        }
    }
}
