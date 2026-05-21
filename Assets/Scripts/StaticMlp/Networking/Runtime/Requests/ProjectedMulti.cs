using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public struct ProjectedMulti<TValue> : IMultiComponent
        where TValue : struct, IMultiComponent
    {
        public TValue Value;

        public ProjectedMulti(TValue value)
        {
            Value = value;
        }
    }
}
