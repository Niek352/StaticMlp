using System;
using System.Collections.Generic;
using NUnit.Framework;
using StaticMlp.Features.Combat;
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
                if (module.SlotKind != EquipmentSlotKind.Combat)
                {
                    Assert.That(module.HasArchetype, Is.False);
                    Assert.That(module.HasGrantedAbility, Is.False);
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        _ = module.SlotType;
                    });
                    continue;
                }

                Assert.That(module.HasArchetype, Is.True);
                Assert.That(module.HasGrantedAbility, Is.True);
                Assert.That(module.EffectKind, Is.EqualTo(LoadoutModuleEffectKind.CombatAbility));
                Assert.That(module.SlotType, Is.EqualTo(LoadoutModuleSlotType.PrimaryAbility));
            }
        }

        [Test]
        public void LoadoutModuleCatalog_SettlementModules_HaveExpectedSlotKindsAndEffects()
        {
            AssertModule(
                LoadoutModuleCatalog.ConstructionUtilityModuleId,
                EquipmentSlotKind.Utility,
                LoadoutModuleEffectKind.ConstructionLogisticsUtility);
            AssertModule(
                LoadoutModuleCatalog.LogisticsUtilityModuleId,
                EquipmentSlotKind.Utility,
                LoadoutModuleEffectKind.SettlementLogisticsUtility);
            AssertModule(
                LoadoutModuleCatalog.BuilderPrioritySignalModuleId,
                EquipmentSlotKind.BuildSignal,
                LoadoutModuleEffectKind.BuilderPrioritySignal);
            AssertModule(
                LoadoutModuleCatalog.HaulerPrioritySignalModuleId,
                EquipmentSlotKind.BuildSignal,
                LoadoutModuleEffectKind.HaulerPrioritySignal);
            AssertModule(
                LoadoutModuleCatalog.StockpileCapacityModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.StorageCapacity);
            AssertModule(
                LoadoutModuleCatalog.BedEfficiencyModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.BedEfficiency);
            AssertModule(
                LoadoutModuleCatalog.RepairEfficiencyModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.RepairEfficiency);
            AssertModule(
                LoadoutModuleCatalog.ExtractionYieldModuleId,
                EquipmentSlotKind.BaseInfrastructure,
                LoadoutModuleEffectKind.ExtractionYield);
        }

        [Test]
        public void ValidateModuleCatalog_CombatModuleWithoutAbility_Throws()
        {
            var invalid = new[]
            {
                new LoadoutModuleDefinition(
                    new LoadoutModuleId(500),
                    EquipmentSlotKind.Combat,
                    LoadoutModuleEffectKind.CombatAbility)
            };

            Assert.Throws<InvalidOperationException>(() => SlotRules.ValidateModuleCatalog(invalid));
        }

        [Test]
        public void ValidateModuleCatalog_NonCombatModuleWithCombatAbility_Throws()
        {
            var invalid = new[]
            {
                new LoadoutModuleDefinition(
                    new LoadoutModuleId(501),
                    EquipmentSlotKind.Utility,
                    LoadoutArchetypeCatalog.PoisonArcherId,
                    CombatAbilityId.PoisonArrow)
            };

            Assert.Throws<InvalidOperationException>(() => SlotRules.ValidateModuleCatalog(invalid));
        }

        [Test]
        public void Stage1LoadoutRules_CreatePreparedSnapshot_RejectsNonCombatModuleSelection()
        {
            var selection = new OwnerLoadoutSelection
            {
                PrimaryModuleId = LoadoutModuleCatalog.ConstructionUtilityModuleId
            };

            Assert.Throws<InvalidOperationException>(() => Stage1LoadoutRules.CreatePreparedSnapshot(selection));
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
        public void ActiveLoadout_SettlementModules_ActivateOnlyInDeclaredSlotKinds()
        {
            var loadout = new ActiveModuleLoadout();
            var utility = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.ConstructionUtilityModuleId);
            var buildSignal = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.BuilderPrioritySignalModuleId);
            var baseInfrastructure = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.StockpileCapacityModuleId);

            Assert.That(ActiveModuleLoadoutRules.CanActivate(loadout, utility, EquipmentSlotKind.Utility, 0), Is.True);
            Assert.That(ActiveModuleLoadoutRules.CanActivate(loadout, utility, EquipmentSlotKind.Combat, 0), Is.False);
            Assert.That(ActiveModuleLoadoutRules.CanActivate(loadout, buildSignal, EquipmentSlotKind.BuildSignal, 0), Is.True);
            Assert.That(ActiveModuleLoadoutRules.CanActivate(loadout, buildSignal, EquipmentSlotKind.Utility, 0), Is.False);
            Assert.That(ActiveModuleLoadoutRules.CanActivate(loadout, baseInfrastructure, EquipmentSlotKind.BaseInfrastructure, 0), Is.True);
            Assert.That(ActiveModuleLoadoutRules.CanActivate(loadout, baseInfrastructure, EquipmentSlotKind.BuildSignal, 0), Is.False);

            ActiveModuleLoadoutRules.Activate(ref loadout, utility, EquipmentSlotKind.Utility, 0);
            ActiveModuleLoadoutRules.Activate(ref loadout, buildSignal, EquipmentSlotKind.BuildSignal, 0);
            ActiveModuleLoadoutRules.Activate(ref loadout, baseInfrastructure, EquipmentSlotKind.BaseInfrastructure, 0);

            Assert.That(loadout.Utility0, Is.EqualTo(LoadoutModuleCatalog.ConstructionUtilityModuleId));
            Assert.That(loadout.BuildSignal0, Is.EqualTo(LoadoutModuleCatalog.BuilderPrioritySignalModuleId));
            Assert.That(loadout.BaseInfrastructure0, Is.EqualTo(LoadoutModuleCatalog.StockpileCapacityModuleId));
            Assert.Throws<InvalidOperationException>(() =>
                ActiveModuleLoadoutRules.Activate(ref loadout, utility, EquipmentSlotKind.Combat, 0));
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

        [Test]
        public void ActiveLoadout_CombatModules_StillFollowCombatLimit()
        {
            var loadout = new ActiveModuleLoadout();
            var poisonArrow = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.PoisonArrowModuleId);
            var fireFlask = LoadoutModuleCatalog.Get(LoadoutModuleCatalog.FireFlaskModuleId);

            ActiveModuleLoadoutRules.Activate(ref loadout, poisonArrow, EquipmentSlotKind.Combat, 0);
            ActiveModuleLoadoutRules.Activate(ref loadout, fireFlask, EquipmentSlotKind.Combat, SlotRuleCatalog.COMBAT_LIMIT - 1);

            Assert.That(loadout.Combat0, Is.EqualTo(LoadoutModuleCatalog.PoisonArrowModuleId));
            Assert.That(loadout.Combat2, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
            Assert.Throws<InvalidOperationException>(() =>
                ActiveModuleLoadoutRules.Activate(ref loadout, poisonArrow, EquipmentSlotKind.Combat, SlotRuleCatalog.COMBAT_LIMIT));
        }

        private static void AssertModule(
            LoadoutModuleId id,
            EquipmentSlotKind expectedSlotKind,
            LoadoutModuleEffectKind expectedEffectKind)
        {
            var module = LoadoutModuleCatalog.Get(id);

            Assert.That(module.SlotKind, Is.EqualTo(expectedSlotKind));
            Assert.That(module.EffectKind, Is.EqualTo(expectedEffectKind));
            Assert.That(module.HasArchetype, Is.False);
            Assert.That(module.HasGrantedAbility, Is.False);
        }
    }
}
