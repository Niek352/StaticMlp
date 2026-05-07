using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiMovementSystem : ISystem
    {
        private const float MoveSpeed = 3.5f;

        public void Update()
        {
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiMoveRequest, CharacterNetState>>().Entities())
            {
                ref readonly var request = ref entity.Read<AiMoveRequest>();
                ref readonly var currentState = ref entity.Read<CharacterNetState>();

                var toTarget = request.Destination - currentState.Position;
                var distance = toTarget.magnitude;
                if (distance <= request.StopDistance)
                {
                    if (currentState.Velocity != Vector3.zero)
                    {
                        ref var stopState = ref ReplicationMut.Mut<CharacterNetState>(entity);
                        stopState.Velocity = Vector3.zero;
                    }

                    entity.Delete<AiMoveRequest>();
                    continue;
                }

                var direction = toTarget / distance;
                var velocity = direction * MoveSpeed;
                ref var state = ref ReplicationMut.Mut<CharacterNetState>(entity);
                state.Velocity = velocity;
                state.Position += velocity * Time.deltaTime;
                state.Rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}
