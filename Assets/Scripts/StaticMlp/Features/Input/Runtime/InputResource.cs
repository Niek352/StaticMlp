using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StaticMlp.Features.Input
{
    public sealed class InputResource : IResource, IDisposable
    {
        private readonly InputSystem_Actions _inputActions;
        private readonly InputAction[] _actions;
        private readonly InputActionName[] _actionNames;
        private readonly bool[] _buttonActions;
        private readonly CachedActionKind[] _actionKinds;
        private readonly int _pointerPositionIndex;

        public InputResource()
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Disable();

            var gameplayMap = _inputActions.Player.Get();
            var actions = gameplayMap.actions;
            _actions = new InputAction[actions.Count];
            _actionNames = new InputActionName[actions.Count];
            _buttonActions = new bool[actions.Count];
            _actionKinds = new CachedActionKind[actions.Count];
            _pointerPositionIndex = -1;

            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                _actions[i] = action;
                _actionNames[i] = new InputActionName(action.name);

                var kind = ResolveKind(action);
                _actionKinds[i] = kind;
                _buttonActions[i] = kind == CachedActionKind.Button;

                if (_actionNames[i] == CoreInputActions.PointerPosition)
                    _pointerPositionIndex = i;
            }
        }

        public Camera AimCamera { get; set; }

        public int DefinitionVersion => 1;

        public void Enable()
        {
            _inputActions.Disable();
            _inputActions.Player.Enable();
        }

        public void Disable()
        {
            _inputActions.Player.Disable();
        }

        public void Dispose()
        {
            Disable();
            _inputActions.Dispose();
        }

        public void Configure(ClientInputState inputState)
        {
            if (inputState == null)
                throw new ArgumentNullException(nameof(inputState));

            inputState.Configure(DefinitionVersion, _actionNames, _buttonActions);
        }

        public void CaptureInto(ClientInputState inputState)
        {
            if (inputState == null)
                throw new ArgumentNullException(nameof(inputState));

            if (inputState.DefinitionVersion != DefinitionVersion)
                Configure(inputState);

            var pointerPosition = GetResolvedPointerPosition();
            var hasAimRay = TryCreateAimRay(pointerPosition, out var aimRay);
            inputState.BeginFrame(pointerPosition, hasAimRay, in aimRay);

            for (var i = 0; i < _actions.Length; i++)
            {
                switch (_actionKinds[i])
                {
                    case CachedActionKind.Button:
                        inputState.UpdateButton(i, _actions[i].IsPressed());
                        break;
                    case CachedActionKind.Vector2:
                        inputState.UpdateVector2(
                            i,
                            i == _pointerPositionIndex
                                ? pointerPosition
                                : _actions[i].ReadValue<Vector2>());
                        break;
                }
            }
        }

        private Vector2 GetResolvedPointerPosition()
        {
            if (_pointerPositionIndex < 0 || Cursor.lockState == CursorLockMode.Locked)
                return GetScreenCenter();

            return _actions[_pointerPositionIndex].ReadValue<Vector2>();
        }

        private bool TryCreateAimRay(Vector2 pointerPosition, out Ray ray)
        {
            if (AimCamera == null)
            {
                ray = default;
                return false;
            }

            ray = AimCamera.ScreenPointToRay(pointerPosition);
            return true;
        }

        private static CachedActionKind ResolveKind(InputAction action)
        {
            if (action == null)
                return CachedActionKind.None;

            if (action.type == InputActionType.Button
                || string.Equals(action.expectedControlType, "Button", StringComparison.Ordinal))
            {
                return CachedActionKind.Button;
            }

            if (string.Equals(action.expectedControlType, "Vector2", StringComparison.Ordinal))
                return CachedActionKind.Vector2;

            return CachedActionKind.None;
        }

        private static Vector2 GetScreenCenter()
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        private enum CachedActionKind : byte
        {
            None,
            Button,
            Vector2
        }
    }
}
