using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine.AI;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiRuntimeInitSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(AiActionCatalog.Discover(new AiTaskExecutionTransitions()));
            ValidateNavigationEnvironment();
            SW.SetResource(AiNavigationRuntime.CreateDefault());
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
