using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game.Input
{
    public sealed class ClientCameraState : IResource
    {
        public float Yaw;
        public float Pitch;
        public bool CursorLocked;
        public bool IsInitialized;
    }
}
