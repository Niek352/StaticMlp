using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Systems.Client {
    public sealed class RemoteInterpolatedViewSyncSystem : ISystem {
        public void Update() {
            foreach (var e in CW.Query<All<RemoteOwned, Interpolated<CharacterNetState>, ViewTransform>>().Entities()) {
                var state = e.Read<Interpolated<CharacterNetState>>().Value;
                ref var view = ref e.Mut<ViewTransform>();

                view.RenderPosition = state.Position;
                view.RenderRotation = state.Rotation;
            }

            foreach (var e in CW.Query<All<RemoteOwned, Interpolated<PhysicsCubeNetState>, ViewTransform>>().Entities()) {
                var state = e.Read<Interpolated<PhysicsCubeNetState>>().Value;
                ref var view = ref e.Mut<ViewTransform>();

                view.RenderPosition = state.Position;
                view.RenderRotation = state.Rotation;
            }
        }
    }
}
