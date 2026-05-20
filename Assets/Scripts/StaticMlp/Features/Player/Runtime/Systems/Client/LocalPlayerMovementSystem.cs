using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public sealed class LocalPlayerMovementSystem : ISystem
    {
        private readonly float _speed;

        public LocalPlayerMovementSystem(float speed = 20f)
        {
            _speed = speed;
        }

        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            var cameraState = CW.GetResource<ClientCameraState>();
            var heightSampler = CW.GetResource<IHeightSampler>();

            foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                ref var state = ref ReplicationMut.Mut<CharacterNetState>(e);

                var input = inputState.ReadVector2(CoreInputActions.Move);
                var cameraYaw = Quaternion.Euler(0f, cameraState.Yaw, 0f);
                var move = cameraYaw * new Vector3(input.x, 0f, input.y);

                if (move.sqrMagnitude > 1f)
                    move.Normalize();

                state.Velocity = move * _speed;
                state.Position += state.Velocity * Time.deltaTime;
                state.Position.y = heightSampler.SampleHeight(state.Position.x, state.Position.z);

                if (state.Velocity.sqrMagnitude > 0.0001f)
                    state.Rotation = Quaternion.LookRotation(state.Velocity);
            }
        }
    }
}
