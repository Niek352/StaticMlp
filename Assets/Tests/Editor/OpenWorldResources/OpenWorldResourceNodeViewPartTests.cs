using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;
using UnityEngine;

namespace StaticMlp.Tests.OpenWorldResources
{
    public sealed class OpenWorldResourceNodeViewPartTests
    {
        [Test]
        public void Apply_WoodState_CreatesWoodVisualAndAmountIndicator()
        {
            var root = new GameObject("Resource Node View Test");
            try
            {
                var part = root.AddComponent<OpenWorldResourceNodeViewPart>();
                part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = OpenWorldGenerationConfig.TREE_RESOURCE_KIND,
                    RemainingAmount = 3,
                    MaxAmount = 5,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                });

                var visual = root.transform.Find("Resource Node Visual");
                Assert.That(visual, Is.Not.Null);
                Assert.That(visual.gameObject.activeSelf, Is.True);
                Assert.That(visual.Find("Wood Trunk"), Is.Not.Null);
                Assert.That(visual.Find("Wood Canopy"), Is.Not.Null);
                Assert.That(CountActivePips(visual), Is.EqualTo(3));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Apply_StoneState_CreatesStoneVisualAndFullAmountIndicator()
        {
            var root = new GameObject("Resource Node View Test");
            try
            {
                var part = root.AddComponent<OpenWorldResourceNodeViewPart>();
                part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = OpenWorldGenerationConfig.ORE_RESOURCE_KIND,
                    RemainingAmount = 8,
                    MaxAmount = 8,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                });

                var visual = root.transform.Find("Resource Node Visual");
                Assert.That(visual, Is.Not.Null);
                Assert.That(visual.gameObject.activeSelf, Is.True);
                Assert.That(visual.Find("Stone Boulder"), Is.Not.Null);
                Assert.That(CountActivePips(visual), Is.EqualTo(5));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(OpenWorldGenerationConfig.SPORE_POD_RESOURCE_KIND, 4, "Spore Stem", "Spore Cap")]
        [TestCase(OpenWorldGenerationConfig.CHEST_RESOURCE_KIND, 6, "Resource Chest Body", "Resource Chest Band")]
        public void Apply_Phase2State_CreatesDistinctVisualAndAmountIndicator(
            ushort kindId,
            int maxAmount,
            string primaryVisualName,
            string secondaryVisualName)
        {
            var root = new GameObject("Resource Node View Test");
            try
            {
                var part = root.AddComponent<OpenWorldResourceNodeViewPart>();
                part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = kindId,
                    RemainingAmount = maxAmount,
                    MaxAmount = maxAmount,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                });

                var visual = root.transform.Find("Resource Node Visual");
                Assert.That(visual, Is.Not.Null);
                Assert.That(visual.gameObject.activeSelf, Is.True);
                Assert.That(visual.Find(primaryVisualName), Is.Not.Null);
                Assert.That(visual.Find(secondaryVisualName), Is.Not.Null);
                Assert.That(CountActivePips(visual), Is.EqualTo(5));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Apply_InactiveFlags_HidesVisualRoot()
        {
            AssertInactiveFlagHidesVisual(OpenWorldResourceOverlayFlags.Depleted);
            AssertInactiveFlagHidesVisual(OpenWorldResourceOverlayFlags.Hidden);
            AssertInactiveFlagHidesVisual(OpenWorldResourceOverlayFlags.Replaced);
        }

        [Test]
        public void Apply_ChangingKind_RebuildsVisual()
        {
            var root = new GameObject("Resource Node View Test");
            try
            {
                var part = root.AddComponent<OpenWorldResourceNodeViewPart>();
                part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = OpenWorldGenerationConfig.TREE_RESOURCE_KIND,
                    RemainingAmount = 5,
                    MaxAmount = 5,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                });

                part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = OpenWorldGenerationConfig.CHEST_RESOURCE_KIND,
                    RemainingAmount = 6,
                    MaxAmount = 6,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                });

                Assert.That(root.transform.childCount, Is.EqualTo(1));
                var visual = root.transform.Find("Resource Node Visual");
                Assert.That(visual, Is.Not.Null);
                Assert.That(visual.Find("Wood Trunk"), Is.Null);
                Assert.That(visual.Find("Resource Chest Body"), Is.Not.Null);
                Assert.That(CountActivePips(visual), Is.EqualTo(5));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Apply_InvalidState_Throws()
        {
            var root = new GameObject("Resource Node View Test");
            try
            {
                var part = root.AddComponent<OpenWorldResourceNodeViewPart>();
                Assert.Throws<System.InvalidOperationException>(() => part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = 99,
                    RemainingAmount = 1,
                    MaxAmount = 1,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                }));
                Assert.Throws<System.InvalidOperationException>(() => part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = OpenWorldGenerationConfig.TREE_RESOURCE_KIND,
                    RemainingAmount = 1,
                    MaxAmount = 0,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    Scale = 1f
                }));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void AssertInactiveFlagHidesVisual(OpenWorldResourceOverlayFlags flag)
        {
            var root = new GameObject("Resource Node View Test");
            try
            {
                var part = root.AddComponent<OpenWorldResourceNodeViewPart>();
                part.Apply(new OpenWorldResourceNodeViewState
                {
                    KindIdValue = OpenWorldGenerationConfig.TREE_RESOURCE_KIND,
                    RemainingAmount = 5,
                    MaxAmount = 5,
                    Flags = flag,
                    Scale = 1f
                });

                var visual = root.transform.Find("Resource Node Visual");
                Assert.That(visual, Is.Not.Null);
                Assert.That(visual.gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static int CountActivePips(Transform visual)
        {
            var indicator = visual.Find("Resource Amount Indicator");
            Assert.That(indicator, Is.Not.Null);

            var active = 0;
            for (var i = 0; i < indicator.childCount; i++)
                if (indicator.GetChild(i).gameObject.activeSelf)
                    active++;

            return active;
        }
    }
}
