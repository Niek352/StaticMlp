using NUnit.Framework;
using StaticMlp.Editor.Ai;
using StaticMlp.Features.AiActions;
using StaticMlp.Features.Settlement.Workers;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiEditorUtilityBindingNameCatalogTests
    {
        [Test]
        public void Catalog_ResolvesVariableNamesFromVariableBindingsFields()
        {
            Assert.That(
                AiEditorUtilityBindingNameCatalog.GetVariableName(AttackEnemyVariableBindings.EnemyDistance01),
                Is.EqualTo("enemy_distance01"));

            Assert.That(
                AiEditorUtilityBindingNameCatalog.GetVariableName(BuildConstructionVariableBindings.HAS_BUILD_TARGET),
                Is.EqualTo("has_build_target"));
        }

        [Test]
        public void Catalog_UsesFallbackWhenVariableNameIsUnknown()
        {
            Assert.That(AiEditorUtilityBindingNameCatalog.GetVariableName(65530), Is.EqualTo("var_65530"));
        }
    }
}
