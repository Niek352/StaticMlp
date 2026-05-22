using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingManagementOperationOpenIntent : IResource
    {
        public bool HasIntent;
        public EntityGID Target;
        public BuildingInteractionKind Kind;

        public void Set(EntityGID target, BuildingInteractionKind kind)
        {
            HasIntent = true;
            Target = target;
            Kind = kind;
        }

        public void Clear()
        {
            HasIntent = false;
            Target = default;
            Kind = default;
        }
    }
}
