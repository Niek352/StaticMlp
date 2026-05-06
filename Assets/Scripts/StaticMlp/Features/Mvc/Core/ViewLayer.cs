using System.Collections.Generic;

namespace Code.EcsUi.Mvc
{
    public enum ViewLayer
    {
        Persistent = 0,
        Fullscreen = 1,
        Popup = 2,
        Overlay = 3,
    }

    public readonly struct ViewOrdering
    {
        private static readonly IReadOnlyDictionary<ViewLayer, int> LayerOffsets = new Dictionary<ViewLayer, int>
        {
            { ViewLayer.Persistent, 0 },
            { ViewLayer.Fullscreen, 200 },
            { ViewLayer.Popup, 400 },
            { ViewLayer.Overlay, 600 },
        };

        public readonly ViewLayer Layer;
        public readonly int OrderInLayer;

        public ViewOrdering(ViewLayer layer, int orderInLayer)
        {
            Layer = layer;
            OrderInLayer = orderInLayer + LayerOffsets[layer];
        }
    }
}
