using System;
using System.Collections.Generic;
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

        [Test]
        public void ActiveLoadout_ActivateWrongSlotKind_Throws()
        {
            var loadout = new ActiveBuildModuleLoadout();
            var module = BuildModuleCatalog.Get(BuildModuleCatalog.PoisonArrowModuleId);

            Assert.Throws<InvalidOperationException>(() =>
                ActiveBuildModuleLoadoutRules.Activate(ref loadout, module, EquipmentSlotKind.Utility, 0));
        }

        [Test]
        public void ActiveLoadout_ActivateOverSlotLimit_Throws()
        {
            var loadout = new ActiveBuildModuleLoadout();
            var module = BuildModuleCatalog.Get(BuildModuleCatalog.PoisonArrowModuleId);

            Assert.Throws<InvalidOperationException>(() =>
                ActiveBuildModuleLoadoutRules.Activate(ref loadout, module, EquipmentSlotKind.Combat, SlotRuleCatalog.COMBAT_LIMIT));
        }

        [Test]
        public void ActiveLoadout_ActiveModules_AreQueryable()
        {
            var loadout = new ActiveBuildModuleLoadout();
            var poisonArrow = BuildModuleCatalog.Get(BuildModuleCatalog.PoisonArrowModuleId);
            var fireFlask = BuildModuleCatalog.Get(BuildModuleCatalog.FireFlaskModuleId);

            ActiveBuildModuleLoadoutRules.Activate(ref loadout, poisonArrow, EquipmentSlotKind.Combat, 0);
            ActiveBuildModuleLoadoutRules.Activate(ref loadout, fireFlask, EquipmentSlotKind.Combat, 1);

            var activeModules = new List<BuildModuleId>();
            ActiveBuildModuleLoadoutQuery.CopyActiveModules(loadout, activeModules);

            Assert.That(activeModules, Is.EqualTo(new[]
            {
                BuildModuleCatalog.PoisonArrowModuleId,
                BuildModuleCatalog.FireFlaskModuleId
            }));
        }
    }
}
