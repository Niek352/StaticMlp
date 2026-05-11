using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public struct Stage1SettlementWorkerSeed
    {
        public ushort AnchorId;
        public ushort RoleId;
        public Vector3 Position;
        public Quaternion Rotation;

        public SettlementAnchorId Anchor => new(AnchorId);
        public WorkerRoleId Role => new(RoleId);
    }
}
