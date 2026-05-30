using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Examples
{
    public struct ExampleUnitHealth : IComponent, ITrackableAdded, ITrackableChanged
    {
        public int Current;
        public int Max;
    }
}
