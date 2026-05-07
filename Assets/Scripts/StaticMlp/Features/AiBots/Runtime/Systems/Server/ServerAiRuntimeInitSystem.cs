using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine.AI;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiRuntimeInitSystem : ISystem
    {
        public void Update()
        {
            EnsureBehaviorCatalog();
            EnsureNavigationRuntime();
        }

        private static void EnsureBehaviorCatalog()
        {
            if (!SW.HasResource<AiBehaviorCatalog>())
            {
                SW.SetResource(AiBehaviorCatalogDefaults.Create());
                return;
            }

            var existingCatalog = SW.GetResource<AiBehaviorCatalog>();
            if (existingCatalog == null)
                throw new InvalidOperationException("AI behavior catalog resource exists but is null.");
        }

        private static void EnsureNavigationRuntime()
        {
            if (!SW.HasResource<AiNavigationRuntime>())
            {
                ValidateNavigationEnvironment();
                SW.SetResource(AiNavigationRuntime.CreateDefault());
                return;
            }

            var runtime = SW.GetResource<AiNavigationRuntime>();
            if (runtime == null)
                throw new InvalidOperationException("AI navigation runtime resource exists but is null.");
        }

        private static void ValidateNavigationEnvironment()
        {
            ValidateBakedNavMesh();
        }

        private static void ValidateBakedNavMesh()
        {
            var triangulation = NavMesh.CalculateTriangulation();
            if (triangulation.vertices != null && triangulation.vertices.Length > 0)
                return;

            throw new InvalidOperationException(
                "AI navigation requires a baked NavMesh. Add a NavMeshSurface to the scene, bake it in the Unity Editor, and then restart the server.");
        }
    }
}
