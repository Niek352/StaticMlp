using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public interface IRequestSystemRegistration
    {
        short ServerOrder { get; }
        short ClientResultOrder { get; }
        bool HasProjector { get; }
        ISystem CreateServerSystem();
        ISystem CreateClientResultSystem();
    }
}
