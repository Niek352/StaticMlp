using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Client-side focus volume for an interactable entity.
    /// The owning feature writes position/radius from its own gameplay transform.
    /// </summary>
    public struct InteractableFocusPoint : IComponent
    {
        public Vector3 Position;
        public float Radius;
    }
}
