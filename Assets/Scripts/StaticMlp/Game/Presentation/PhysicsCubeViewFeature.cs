using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.EcsViews;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Presentation
{
    public sealed class PhysicsCubeViewFeature : GameplayFeature
    {
        private const string PhysicsCubeViewPath = "Views/PhysicsCubeView";

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(BuiltinGameplayFeature.PHYSICS_CUBE, e =>
            {
                e.Set(new ViewPath(PhysicsCubeViewPath));
            });
        }
    }
}
