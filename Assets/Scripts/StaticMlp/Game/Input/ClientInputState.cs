using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Game.Input
{
    public sealed class ClientInputState : IResource
    {
        private readonly Dictionary<string, int> _indices = new(StringComparer.Ordinal);

        private InputActionName[] _actions = Array.Empty<InputActionName>();
        private bool[] _buttonActions = Array.Empty<bool>();
        private Vector2[] _vector2Values = Array.Empty<Vector2>();
        private bool[] _pressed = Array.Empty<bool>();
        private bool[] _wasPressed = Array.Empty<bool>();
        private bool[] _wasReleased = Array.Empty<bool>();

        private Ray _aimRay;
        private bool _hasAimRay;

        public int DefinitionVersion { get; private set; } = -1;
        public int ActionCount => _actions.Length;
        public Vector2 PointerPosition { get; private set; }

        public void Configure(int definitionVersion, InputActionName[] actions, bool[] buttonActions)
        {
            DefinitionVersion = definitionVersion;
            _indices.Clear();

            if (actions == null
                || buttonActions == null
                || actions.Length != buttonActions.Length)
            {
                _actions = Array.Empty<InputActionName>();
                _buttonActions = Array.Empty<bool>();
                _vector2Values = Array.Empty<Vector2>();
                _pressed = Array.Empty<bool>();
                _wasPressed = Array.Empty<bool>();
                _wasReleased = Array.Empty<bool>();
                PointerPosition = default;
                _aimRay = default;
                _hasAimRay = false;
                return;
            }

            _actions = new InputActionName[actions.Length];
            _buttonActions = new bool[buttonActions.Length];
            _vector2Values = new Vector2[actions.Length];
            _pressed = new bool[actions.Length];
            _wasPressed = new bool[actions.Length];
            _wasReleased = new bool[actions.Length];

            Array.Copy(actions, _actions, actions.Length);
            Array.Copy(buttonActions, _buttonActions, buttonActions.Length);

            for (var i = 0; i < _actions.Length; i++)
            {
                if (!_actions[i].IsEmpty)
                    _indices[_actions[i].Value] = i;
            }

            PointerPosition = default;
            _aimRay = default;
            _hasAimRay = false;
        }

        public void BeginFrame(Vector2 pointerPosition, bool hasAimRay, in Ray aimRay)
        {
            Array.Clear(_vector2Values, 0, _vector2Values.Length);
            Array.Clear(_wasPressed, 0, _wasPressed.Length);
            Array.Clear(_wasReleased, 0, _wasReleased.Length);

            PointerPosition = pointerPosition;
            _hasAimRay = hasAimRay;
            _aimRay = aimRay;
        }

        public void ResetAll()
        {
            Array.Clear(_vector2Values, 0, _vector2Values.Length);
            Array.Clear(_wasPressed, 0, _wasPressed.Length);
            Array.Clear(_wasReleased, 0, _wasReleased.Length);

            for (var i = 0; i < _pressed.Length; i++)
            {
                if (_pressed[i])
                    _wasReleased[i] = true;

                _pressed[i] = false;
            }

            PointerPosition = default;
            _aimRay = default;
            _hasAimRay = false;
        }

        public void UpdateVector2(int index, Vector2 value)
        {
            if ((uint)index >= (uint)_vector2Values.Length)
                return;

            _vector2Values[index] = value;
        }

        public void UpdateButton(int index, bool isPressed)
        {
            if ((uint)index >= (uint)_pressed.Length || !_buttonActions[index])
                return;

            var wasPressed = _pressed[index];
            _pressed[index] = isPressed;
            _wasPressed[index] = !wasPressed && isPressed;
            _wasReleased[index] = wasPressed && !isPressed;
        }

        public Vector2 ReadVector2(InputActionName action)
        {
            return _vector2Values[GetIndex(action)];
        }

        public bool IsPressed(InputActionName action)
        {
            return _pressed[GetIndex(action)];
        }

        public bool WasPressed(InputActionName action)
        {
            return _wasPressed[GetIndex(action)];
        }

        public bool WasReleased(InputActionName action)
        {
            return _wasReleased[GetIndex(action)];
        }

        public bool TryGetAimRay(out Ray ray)
        {
            ray = _aimRay;
            return _hasAimRay;
        }

        public InputActionName GetActionName(int index)
        {
            return (uint)index < (uint)_actions.Length
                ? _actions[index]
                : default;
        }

        public bool IsButtonAction(int index)
        {
            return (uint)index < (uint)_buttonActions.Length && _buttonActions[index];
        }

        public bool WasPressedAt(int index)
        {
            return (uint)index < (uint)_wasPressed.Length && _wasPressed[index];
        }

        public bool WasReleasedAt(int index)
        {
            return (uint)index < (uint)_wasReleased.Length && _wasReleased[index];
        }
        private int GetIndex(in InputActionName action) =>
            _indices[action.Value];
    }
}
