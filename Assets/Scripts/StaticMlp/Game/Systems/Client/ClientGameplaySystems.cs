using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Game.Systems.Client {
    public static class NetworkInput {
        public static Func<Vector2> MoveProvider = () => Vector2.zero;
    }

    public sealed class LocalPlayerMovementSystem : ISystem {
        private readonly float _speed;

        public LocalPlayerMovementSystem(float speed = 5f) {
            _speed = speed;
        }

        public void Update() {
            foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities()) {
                ref var state = ref e.Mut<CharacterNetState>();

                var input = NetworkInput.MoveProvider();
                var move = new Vector3(input.x, 0f, input.y);

                if (move.sqrMagnitude > 1f)
                    move.Normalize();

                state.Velocity = move * _speed;
                state.Position += state.Velocity * Time.deltaTime;

                if (state.Velocity.sqrMagnitude > 0.0001f)
                    state.Rotation = Quaternion.LookRotation(state.Velocity);
            }
        }
    }

    public sealed class RemoteSmoothingSystem : ISystem {
        private readonly float _sharpness;

        public RemoteSmoothingSystem(float sharpness = 12f) {
            _sharpness = sharpness;
        }

        public void Update() {
            foreach (var e in CW.Query<All<RemoteOwned, CharacterNetState, ViewTransform>>().Entities()) {
                ref readonly var net = ref e.Read<CharacterNetState>();
                ref var view = ref e.Mut<ViewTransform>();

                var t = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
                view.RenderPosition = Vector3.Lerp(view.RenderPosition, net.Position, t);
                view.RenderRotation = Quaternion.Slerp(view.RenderRotation, net.Rotation, t);
            }
        }
    }
}
