using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server {
    public sealed class ServerAiSystem : ISystem {
        public void Update() {
            foreach (var e in SW.Query<All<ServerOwned, MonsterTag, CharacterNetState>>().Entities()) {
                ref var state = ref e.Mut<CharacterNetState>();
                state.Rotation = Quaternion.identity;
            }
        }
    }
}
