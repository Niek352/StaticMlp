using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Examples
{
    public struct ExampleInventoryItem : IMultiComponent
    {
        public int Id;
        public int Count;
    }
}
