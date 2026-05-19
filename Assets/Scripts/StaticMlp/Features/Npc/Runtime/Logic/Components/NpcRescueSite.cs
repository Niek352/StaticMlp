using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.Npc
{
    public struct NpcRescueSite : IComponent
    {
        public ushort NpcDefinitionId;
        public NpcRescueSiteState State;
        public Vector3 Position;

        public NpcDefinitionId Definition => new(NpcDefinitionId);
    }
}
