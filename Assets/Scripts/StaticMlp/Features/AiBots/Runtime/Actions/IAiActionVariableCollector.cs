using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public interface IAiActionVariableCollector
    {
        void Collect(SW.Entity entity);
    }
}
