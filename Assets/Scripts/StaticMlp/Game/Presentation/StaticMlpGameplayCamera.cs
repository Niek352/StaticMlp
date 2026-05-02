using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StaticMlp.Game.Presentation
{
    public sealed class StaticMlpGameplayCamera : MonoBehaviour
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

        [Header("Follow")] [SerializeField] private float followSharpness = 20f;

        private float _yaw;
        private float _pitch = 18f;
        private Vector3 _smoothedPivot;
        private bool _hasPivot;

        private void Awake()
        {
            if (targetCamera == null && !TryGetComponent(out targetCamera))
                throw new System.InvalidOperationException("StaticMlpGameplayCamera requires a Camera reference.");

            _yaw = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            NetworkInput.CameraYawProvider = GetCameraYaw;

            if (lockCursorOnEnable)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            NetworkInput.CameraYawProvider = () => 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (EscapeWasPressed())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (RightMouseWasPressedThisFrame() && rotateOnlyWhileRightMouseHeld)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (rotateOnlyWhileRightMouseHeld && !RightMouseIsPressed())
                return;

            var look = ReadLookDelta();
            _yaw += look.x * yawSensitivity;
            _pitch -= look.y * pitchSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private void LateUpdate()
        {
            if (!TryGetLocalPlayerPosition(out var playerPosition))
                return;

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

            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var position = _smoothedPivot - rotation * Vector3.forward * distance;
            targetCamera.transform.SetPositionAndRotation(position, rotation);
        }

        private float GetCameraYaw()
        {
            return _yaw;
        }

        private static Vector2 ReadLookDelta()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.delta.ReadValue() * 0.08f;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }

        private static bool EscapeWasPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private static bool RightMouseWasPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
                return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(1);
#else
            return false;
#endif
        }

        private static bool RightMouseIsPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.rightButton.isPressed;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(1);
#else
            return false;
#endif
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