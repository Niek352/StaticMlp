using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.EcsViews
{
    public readonly struct ViewPath : IComponent
    {
        public readonly string Value;

        public ViewPath(string value)
        {
            Value = value;
        }
    }
}
