using System;
using UnityEngine;

namespace StaticMlp.Game.Systems.Client {
    public static class NetworkInput {
        public static Func<Vector2> MoveProvider = () => Vector2.zero;
        public static Func<float> CameraYawProvider = () => 0f;
        public static Func<bool> SpawnCubeWasPressedProvider = () => false;
    }
}
