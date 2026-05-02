using System;
using UnityEngine;

namespace StaticMlp.Game.Systems.Client {
    public static class NetworkInput {
        public static Func<Vector2> MoveProvider = () => Vector2.zero;
    }
}
