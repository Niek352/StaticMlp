using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.EcsViews;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class BotViewPartTests
    {
        [Test]
        public void Apply_WithHalfHealth_ScalesFillToHalfWidth()
        {
            var root = new GameObject("Bot View Test");
            try
            {
                var part = root.AddComponent<StaticMlp.Features.AiBots.BotViewPart>();
                part.OnBind(new TestEntityView());
                part.Apply(new CombatHealthViewState
                {
                    HealthNormalized = 0.5f,
                    IsDead = false
                });

                var fill = root.transform.Find("Health Bar Root/Health Bar Fill");
                Assert.That(fill, Is.Not.Null);
                Assert.That(fill.gameObject.activeSelf, Is.True);
                Assert.That(fill.localScale.x, Is.EqualTo(0.6f).Within(0.001f));
                Assert.That(fill.localPosition.x, Is.EqualTo(-0.3f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Apply_WithZeroHealth_HidesFill()
        {
            var root = new GameObject("Bot View Test");
            try
            {
                var part = root.AddComponent<StaticMlp.Features.AiBots.BotViewPart>();
                part.OnBind(new TestEntityView());
                part.Apply(new CombatHealthViewState
                {
                    HealthNormalized = 0f,
                    IsDead = true
                });

                var fill = root.transform.Find("Health Bar Root/Health Bar Fill");
                Assert.That(fill, Is.Not.Null);
                Assert.That(fill.gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private sealed class TestEntityView : IEntityView
        {
            public CW.Entity Entity => default;

            public void Bind(CW.Entity entity)
            {
            }

            public void Unbind()
            {
            }

            public void Apply<TComponent>(in TComponent component)
                where TComponent : struct, IViewComponent
            {
            }
        }
    }
}
