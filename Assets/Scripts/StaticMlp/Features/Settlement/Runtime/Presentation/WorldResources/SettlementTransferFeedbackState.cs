using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementTransferFeedbackState : IResource
    {
        public bool HasMessage;
        public EntityGID Building;
        public string Message;

        public void Set(EntityGID building, string message)
        {
            HasMessage = true;
            Building = building;
            Message = message;
        }

        public void Clear()
        {
            HasMessage = false;
            Building = default;
            Message = string.Empty;
        }
    }
}
