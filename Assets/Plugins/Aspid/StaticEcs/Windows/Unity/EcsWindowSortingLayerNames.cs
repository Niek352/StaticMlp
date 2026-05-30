using System;

namespace Aspid.StaticEcs.Windows
{
    public static class EcsWindowSortingLayerNames
    {
        public static string For(EcsWindowLayer layer)
        {
            return layer switch
            {
                EcsWindowLayer.Persistent => nameof(EcsWindowLayer.Persistent),
                EcsWindowLayer.Fullscreen => nameof(EcsWindowLayer.Fullscreen),
                EcsWindowLayer.Popup => nameof(EcsWindowLayer.Popup),
                EcsWindowLayer.Overlay => nameof(EcsWindowLayer.Overlay),
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
            };
        }
    }
}
