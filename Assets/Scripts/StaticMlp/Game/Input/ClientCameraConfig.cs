using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Game.Input
{
    public sealed class ClientCameraConfig : IResource
    {
        public float YawSensitivity = 4f;
        public float PitchSensitivity = 3f;
        public float MinPitch = -35f;
        public float MaxPitch = 65f;
        public float InitialPitch = 18f;
        public bool LockCursorOnEnable = true;
        public bool RotateOnlyWhileSecondaryHeld;
        public Camera AimCamera;
    }
}
