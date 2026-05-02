using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Game.Systems.Client {
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
}
