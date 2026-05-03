using StaticMlp.Game.Bootstrap;
using StaticMlp.Features.EcsViews;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Features.Builtin
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
