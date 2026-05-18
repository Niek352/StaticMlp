using System;

namespace StaticMlp.Features.Build
{
    public static class SlotRuleCatalog
    {
        public const int COMBAT_LIMIT = 3;
        public const int UTILITY_LIMIT = 2;
        public const int BUILD_SIGNAL_LIMIT = 2;
        public const int BASE_INFRASTRUCTURE_LIMIT = 4;

        public static int GetLimit(EquipmentSlotKind kind)
        {
            return kind switch
            {
                EquipmentSlotKind.Combat => COMBAT_LIMIT,
                EquipmentSlotKind.Utility => UTILITY_LIMIT,
                EquipmentSlotKind.BuildSignal => BUILD_SIGNAL_LIMIT,
                EquipmentSlotKind.BaseInfrastructure => BASE_INFRASTRUCTURE_LIMIT,
                _ => throw new InvalidOperationException($"Unsupported equipment slot kind {kind}.")
            };
        }
    }
}
