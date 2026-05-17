using System;
using System.Collections.Generic;

namespace StaticMlp.LayerProcLite
{
    public sealed class LayerProcLitePlanBuilder
    {
        private readonly Dictionary<LayerProcLiteLayerId, LayerProcLiteLayerDescriptor> _descriptors = new();

        public LayerProcLitePlanBuilder Add(LayerProcLiteLayerDescriptor descriptor) =>
            _descriptors.TryAdd(descriptor.LayerId, descriptor)
                ? this : throw new InvalidOperationException($"Layer {descriptor.LayerId.Value} is already registered.");

        public LayerProcLitePlanStep[] Build(LayerProcLiteLayerMask requestedLayers)
        {
            if (requestedLayers.IsEmpty)
                return Array.Empty<LayerProcLitePlanStep>();

            var state = new Dictionary<LayerProcLiteLayerId, VisitState>();
            var order = new List<LayerProcLiteLayerId>();

            for (var i = 0; i < 64; i++)
            {
                var layerId = new LayerProcLiteLayerId(i);
                if (requestedLayers.Contains(layerId))
                    Visit(layerId, state, order);
            }

            var windows = new Dictionary<LayerProcLiteLayerId, LayerProcLiteWindow>();
            for (var i = 0; i < order.Count; i++)
                windows.Add(order[i], LayerProcLiteWindow.None);

            for (var i = order.Count - 1; i >= 0; i--)
            {
                var layerId = order[i];
                var descriptor = _descriptors[layerId];
                var layerWindow = windows[layerId];

                for (var dependencyIndex = 0; dependencyIndex < descriptor.Dependencies.Length; dependencyIndex++)
                {
                    var dependency = descriptor.Dependencies[dependencyIndex];
                    var dependencyWindow = layerWindow.ExpandedBy(
                        dependency.PaddingSamples,
                        dependency.EffectDistanceWorld);
                    windows[dependency.LayerId] =
                        LayerProcLiteWindow.Max(windows[dependency.LayerId], dependencyWindow);
                }
            }

            var steps = new LayerProcLitePlanStep[order.Count];
            for (var i = 0; i < order.Count; i++)
            {
                var layerId = order[i];
                var descriptor = _descriptors[layerId];
                var dependencies = LayerProcLiteLayerMask.None;
                for (var dependencyIndex = 0; dependencyIndex < descriptor.Dependencies.Length; dependencyIndex++)
                    dependencies = dependencies.With(descriptor.Dependencies[dependencyIndex].LayerId);

                steps[i] = new LayerProcLitePlanStep(layerId, dependencies, windows[layerId]);
            }

            return steps;
        }

        private void Visit(
            LayerProcLiteLayerId layerId,
            Dictionary<LayerProcLiteLayerId, VisitState> state,
            List<LayerProcLiteLayerId> order)
        {
            if (!_descriptors.TryGetValue(layerId, out var descriptor))
                throw new KeyNotFoundException($"Layer {layerId.Value} is not registered.");

            if (state.TryGetValue(layerId, out var currentState))
            {
                if (currentState == VisitState.Visiting)
                    throw new InvalidOperationException($"Layer dependency cycle includes layer {layerId.Value}.");

                return;
            }

            state.Add(layerId, VisitState.Visiting);

            for (var i = 0; i < descriptor.Dependencies.Length; i++)
                Visit(descriptor.Dependencies[i].LayerId, state, order);

            state[layerId] = VisitState.Visited;
            order.Add(layerId);
        }

        private enum VisitState
        {
            Visiting,
            Visited
        }
    }
}