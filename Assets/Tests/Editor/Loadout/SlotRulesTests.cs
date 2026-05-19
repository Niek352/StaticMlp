using System;
using System.Collections.Generic;
using NUnit.Framework;
using StaticMlp.Features.Loadout;

namespace StaticMlp.Tests.Loadout
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
            var module = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.PoisonArrowModuleId);

            Assert.Throws<InvalidOperationException>(() =>
                SlotRules.ValidateModuleSupportsSlotKind(module, EquipmentSlotKind.Utility));
        }

        [Test]
        public void LoadoutModuleCatalog_CurrentCatalog_ValidatesAgainstSlotRules()
        {
            Assert.DoesNotThrow(() => SlotRules.ValidateModuleCatalog(LoadoutModuleCatalog.All));

            foreach (var module in LoadoutModuleCatalog.All)
            {
                Assert.That(module.SlotKind, Is.EqualTo(EquipmentSlotKind.Combat));
                Assert.That(module.SlotType, Is.EqualTo(LoadoutModuleSlotType.PrimaryAbility));
            }
        }

        [Test]
        public void ActiveLoadout_ActivateWrongSlotKind_Throws()
        {
            var loadout = new ActiveModuleLoadout();
            var module = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.PoisonArrowModuleId);

            Assert.Throws<InvalidOperationException>(() =>
                ActiveModuleLoadoutRules.Activate(ref loadout, module, EquipmentSlotKind.Utility, 0));
        }

        [Test]
        public void ActiveLoadout_ActivateOverSlotLimit_Throws()
        {
            var loadout = new ActiveModuleLoadout();
            var module = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.PoisonArrowModuleId);

            Assert.Throws<InvalidOperationException>(() =>
                ActiveModuleLoadoutRules.Activate(ref loadout, module, EquipmentSlotKind.Combat, SlotRuleCatalog.COMBAT_LIMIT));
        }

        [Test]
        public void ActiveLoadout_ActiveModules_AreQueryable()
        {
            var loadout = new ActiveModuleLoadout();
            var poisonArrow = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.PoisonArrowModuleId);
            var fireFlask = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.FireFlaskModuleId);

            ActiveModuleLoadoutRules.Activate(ref loadout, poisonArrow, EquipmentSlotKind.Combat, 0);
            ActiveModuleLoadoutRules.Activate(ref loadout, fireFlask, EquipmentSlotKind.Combat, 1);

            var activeModules = new List<LoadoutModuleId>();
            ActiveModuleLoadoutQuery.CopyActiveModules(loadout, activeModules);

            Assert.That(activeModules, Is.EqualTo(new[]
            {
                LoadoutModuleCatalog.PoisonArrowModuleId,
                LoadoutModuleCatalog.FireFlaskModuleId
            }));
        }
    }
}
