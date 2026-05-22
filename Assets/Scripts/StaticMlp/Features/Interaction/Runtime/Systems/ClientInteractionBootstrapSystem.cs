using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Initialises <see cref="InteractionFocus"/> as a world resource at client startup.
    /// Runs once; no Update logic.
    /// </summary>
    public sealed class ClientInteractionBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new InteractionFocus());
        }
    }
}
