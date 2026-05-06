using System;

namespace Code.EcsUi.Mvc
{
    public static class PrefabSortingLayerNames
    {
        public static string For(ViewLayer layer)
        {
            return layer switch
            {
                ViewLayer.Persistent => nameof(ViewLayer.Persistent),
                ViewLayer.Fullscreen => nameof(ViewLayer.Fullscreen),
                ViewLayer.Popup => nameof(ViewLayer.Popup),
                ViewLayer.Overlay => nameof(ViewLayer.Overlay),
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null),
            };
        }
    }
}
