using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Game.Systems.Client {
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
