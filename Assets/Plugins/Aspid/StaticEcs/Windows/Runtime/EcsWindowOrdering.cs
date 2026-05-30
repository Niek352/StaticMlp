namespace Aspid.StaticEcs.Windows
{
    public readonly struct EcsWindowOrdering
    {
        public readonly EcsWindowLayer Layer;
        public readonly int OrderInLayer;

        public EcsWindowOrdering(EcsWindowLayer layer, int orderInLayer)
        {
            Layer = layer;
            OrderInLayer = orderInLayer;
        }
    }
}
