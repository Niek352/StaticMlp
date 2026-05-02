using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Game.Systems.Client {
    public sealed class LocalViewSyncSystem : ISystem {
        public void Update() {
            foreach (var e in CW.Query<All<LocalOwned, CharacterNetState, ViewTransform>>().Entities()) {
                ref readonly var state = ref e.Read<CharacterNetState>();
                ref var view = ref e.Mut<ViewTransform>();

                view.RenderPosition = state.Position;
                view.RenderRotation = state.Rotation;
            }
        }
    }
}
