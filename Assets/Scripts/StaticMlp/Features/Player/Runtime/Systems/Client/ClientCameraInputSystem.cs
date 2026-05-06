using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public sealed class ClientCameraInputSystem : ISystem
    {
        public void Init()
        {
            CW.GetResource<ClientInputState>();
            CW.SetResource(new ClientCameraConfig());
            CW.SetResource(new ClientCameraState());
        }

        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            var config = CW.GetResource<ClientCameraConfig>();
            var state = CW.GetResource<ClientCameraState>();

            if (!state.IsInitialized)
            {
                state.Yaw = config.AimCamera != null
                    ? config.AimCamera.transform.eulerAngles.y
                    : 0f;
                state.Pitch = config.InitialPitch;
                state.CursorLocked = config.LockCursorOnEnable;
                state.IsInitialized = true;
            }

            if (inputState.WasPressed(CoreInputActions.Cancel))
                state.CursorLocked = false;

            if (config.RotateOnlyWhileSecondaryHeld && inputState.WasPressed(CoreInputActions.Secondary))
                state.CursorLocked = true;

            var canRotate = !config.RotateOnlyWhileSecondaryHeld || inputState.IsPressed(CoreInputActions.Secondary);
            if (!canRotate)
                return;

            var look = inputState.ReadVector2(CoreInputActions.Look);
            state.Yaw += look.x * config.YawSensitivity;
            state.Pitch -= look.y * config.PitchSensitivity;
            state.Pitch = Mathf.Clamp(state.Pitch, config.MinPitch, config.MaxPitch);
        }
    }
}
