using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementBuildingOperationOpenIntent : IResource
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
    }
}
