using System;
using NUnit.Framework;
using StaticMlp.Features.Build;

namespace StaticMlp.Tests.Build
{
    public sealed class SlotRulesTests
    {
        [Test]
        public void SlotRuleCatalog_BaselineLimits_MatchDesignLock()
        {
            Assert.That(SlotRuleCatalog.GetLimit(EquipmentSlotKind.Combat), Is.EqualTo(3));
            Assert.That(SlotRuleCatalog.GetLimit(EquipmentSlotKind.Utility), Is.EqualTo(2));
            Assert.That(SlotRuleCatalog.GetLimit(EquipmentSlotKind.BuildSignal), Is.EqualTo(2));
            Assert.That(SlotRuleCatalog.GetLimit(EquipmentSlotKind.BaseInfrastructure), Is.EqualTo(4));
        }

        [Test]
        public void ValidateSlotKind_InvalidKind_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => SlotRules.ValidateSlotKind(EquipmentSlotKind.None));
            Assert.Throws<InvalidOperationException>(() => SlotRules.ValidateSlotKind((EquipmentSlotKind)99));
        }

        [Test]
        public void ValidateSlotIndex_IndexEqualToLimit_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                SlotRules.ValidateSlotIndex(EquipmentSlotKind.Combat, SlotRuleCatalog.GetLimit(EquipmentSlotKind.Combat)));
        }

        [Test]
        public void ValidateModuleSupportsSlotKind_UnsupportedKind_Throws()
        {
            var module = BuildModuleCatalog.Get(BuildModuleCatalog.PoisonArrowModuleId);

            Assert.Throws<InvalidOperationException>(() =>
                SlotRules.ValidateModuleSupportsSlotKind(module, EquipmentSlotKind.Utility));
        }

        [Test]
        public void BuildModuleCatalog_CurrentCatalog_ValidatesAgainstSlotRules()
        {
            Assert.DoesNotThrow(() => SlotRules.ValidateModuleCatalog(BuildModuleCatalog.All));

            foreach (var module in BuildModuleCatalog.All)
            {
                Assert.That(module.SlotKind, Is.EqualTo(EquipmentSlotKind.Combat));
                Assert.That(module.SlotType, Is.EqualTo(BuildModuleSlotType.PrimaryAbility));
            }
        }
    }
}
