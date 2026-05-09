using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public interface IAiActionCommandTargetBinder
    {
        bool TryBind(SW.Entity bot, EntityGID target);
    }
}
