using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.OpenWorldGeneration
{
    public sealed class OpenWorldTerrainRuntimeTests
    {
        [Test]
        public void StreamAround_RequestsVisualMeshAndPlacements()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(OpenWorldChunkGenerationRequested).Assembly,
                typeof(OpenWorldTerrainRuntime).Assembly);
            CW.Initialize();

            var receiver = CW.RegisterEventReceiver<OpenWorldChunkGenerationRequested>();
            var runtime = OpenWorldTerrainRuntime.Create(new OpenWorldTerrainStreamingConfig
            {
                Bounds = new WorldChunkBounds(0, 0, 0, 0),
                ViewRadiusInChunks = 0,
                ColliderRadiusInChunks = 0,
                MaxChunkLoadsPerFrame = 1,
                ChunkWorldSize = 32f,
                RootName = "Open World Terrain Runtime Test"
            });

            try
            {
                runtime.StreamAround(Vector3.zero);

                var found = false;
                foreach (var evt in receiver)
                {
                    found = true;
                    Assert.That(evt.Value.Outputs.HasFlag(GenerationOutputMask.VisualMesh), Is.True);
                    Assert.That(evt.Value.Outputs.HasFlag(GenerationOutputMask.Placements), Is.True);
                }

                Assert.That(found, Is.True);
            }
            finally
            {
                runtime.Dispose();
                CW.DeleteEventReceiver(ref receiver);
                CW.Destroy();
            }
        }
    }
}
