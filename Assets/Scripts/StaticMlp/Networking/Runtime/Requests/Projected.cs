using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public struct Projected<T> : IComponent where T : struct, IComponent
    {
        public T Value;

        public Projected(T value)
        {
            Value = value;
        }
    }
}
