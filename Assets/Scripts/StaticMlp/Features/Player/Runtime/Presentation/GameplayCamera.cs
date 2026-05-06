using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Input;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    [DefaultExecutionOrder(-200)]
    public sealed class GameplayCamera : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Camera targetCamera;

        [Header("Orbit")] [SerializeField] private Vector3 pivotOffset = new(0f, 1.35f, 0f);
        [SerializeField] private float distance = 6f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 65f;

        [Header("Input")] [SerializeField] private float yawSensitivity = 4f;
        [SerializeField] private float pitchSensitivity = 3f;
        [SerializeField] private bool lockCursorOnEnable = true;
        [SerializeField] private bool rotateOnlyWhileRightMouseHeld;
        [SerializeField] private float initialPitch = 18f;

        [Header("Follow")] [SerializeField] private float followSharpness = 20f;

        private Vector3 _smoothedPivot;
        private bool _hasPivot;

        private void Awake()
        {
            if (targetCamera == null && !TryGetComponent(out targetCamera))
                throw new System.InvalidOperationException("GameplayCamera requires a Camera reference.");
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            SyncCameraResources();
        }

        private void LateUpdate()
        {
            SyncCameraResources();
            if (CW.Status != WorldStatus.Initialized)
                return;

            ApplyCursorState();

            if (!TryGetLocalPlayerPosition(out var playerPosition))
                return;

            var cameraState = GetCameraState();
            if (!cameraState.IsInitialized)
                throw new System.InvalidOperationException("Client camera state was not initialized.");

            var targetPivot = playerPosition + pivotOffset;
            if (!_hasPivot)
            {
                _smoothedPivot = targetPivot;
                _hasPivot = true;
            }
            else
            {
                var t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
                _smoothedPivot = Vector3.Lerp(_smoothedPivot, targetPivot, t);
            }

            var rotation = Quaternion.Euler(cameraState.Pitch, cameraState.Yaw, 0f);
            var position = _smoothedPivot - rotation * Vector3.forward * distance;
            targetCamera.transform.SetPositionAndRotation(position, rotation);
        }

        private void SyncCameraResources()
        {
            if (CW.Status != WorldStatus.Initialized)
                return;

            var config = CW.GetResource<ClientCameraConfig>();
            config.YawSensitivity = yawSensitivity;
            config.PitchSensitivity = pitchSensitivity;
            config.MinPitch = minPitch;
            config.MaxPitch = maxPitch;
            config.InitialPitch = initialPitch;
            config.LockCursorOnEnable = lockCursorOnEnable;
            config.RotateOnlyWhileSecondaryHeld = rotateOnlyWhileRightMouseHeld;

            var input = CW.GetResource<InputResource>();
            input.AimCamera = targetCamera;

            var state = CW.GetResource<ClientCameraState>();
            if (state.IsInitialized)
                return;

            state.Yaw = transform.eulerAngles.y;
            state.Pitch = initialPitch;
            state.CursorLocked = lockCursorOnEnable;
            state.IsInitialized = true;
        }

        private void ApplyCursorState()
        {
            var cameraState = GetCameraState();
            if (!cameraState.IsInitialized)
                throw new System.InvalidOperationException("Client camera state was not initialized.");

            Cursor.lockState = cameraState.CursorLocked
                ? CursorLockMode.Locked
                : CursorLockMode.None;
            Cursor.visible = !cameraState.CursorLocked;
        }

        private static ClientCameraState GetCameraState()
        {
            return CW.GetResource<ClientCameraState>();
        }

        private static bool TryGetLocalPlayerPosition(out Vector3 position)
        {
            position = default;

            if (CW.Status != WorldStatus.Initialized)
                return false;

            foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                position = e.Read<CharacterNetState>().Position;
                return true;
            }

            return false;
        }
    }
}
