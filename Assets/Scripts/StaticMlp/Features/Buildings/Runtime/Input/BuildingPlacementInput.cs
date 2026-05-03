using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StaticMlp.Features.Buildings
{
    public static class BuildingPlacementInput
    {
        public static Func<bool> ToggleMenuWasPressedProvider = DefaultToggleMenuWasPressed;
        public static Func<bool> ConfirmPlacementWasPressedProvider = DefaultConfirmPlacementWasPressed;
        public static Func<bool> CancelPlacementWasPressedProvider = DefaultCancelPlacementWasPressed;
        public static Func<bool> RotatePlacementWasPressedProvider = DefaultRotatePlacementWasPressed;
        public static Func<bool> DepositResourcesWasPressedProvider = DefaultDepositResourcesWasPressed;
        public static Func<bool> BuildConstructionIsPressedProvider = DefaultBuildConstructionIsPressed;
        public static PlacementAimRayProvider AimRayProvider = TryReadDefaultAimRay;

        private static bool DefaultToggleMenuWasPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.bKey.wasPressedThisFrame;
        }

        private static bool DefaultConfirmPlacementWasPressed()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private static bool DefaultCancelPlacementWasPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                return true;

            var mouse = Mouse.current;
            return mouse != null && mouse.rightButton.wasPressedThisFrame;
        }

        private static bool DefaultRotatePlacementWasPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.rKey.wasPressedThisFrame;
        }

        private static bool DefaultDepositResourcesWasPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
        }

        private static bool DefaultBuildConstructionIsPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.fKey.isPressed;
        }

        private static bool TryReadDefaultAimRay(out Ray ray)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                ray = default;
                return false;
            }

            var mouse = Mouse.current;
            var screenPosition = mouse != null
                ? mouse.position.ReadValue()
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            ray = camera.ScreenPointToRay(screenPosition);
            return true;
        }
    }
}
