namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLitePlanStep
    {
        public readonly LayerProcLiteLayerId LayerId;
        public readonly LayerProcLiteLayerMask Dependencies;
        public readonly LayerProcLiteWindow Window;

        public LayerProcLitePlanStep(
            LayerProcLiteLayerId layerId,
            LayerProcLiteLayerMask dependencies,
            LayerProcLiteWindow window)
        {
            LayerId = layerId;
            Dependencies = dependencies;
            Window = window;
        }

        public bool DependsOn(LayerProcLiteLayerId layerId)
        {
            return Dependencies.Contains(layerId);
        }
    }
}
