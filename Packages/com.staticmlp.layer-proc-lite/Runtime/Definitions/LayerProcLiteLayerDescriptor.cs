using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteLayerDescriptor
    {
        public static readonly LayerProcLiteDependency[] NoDependencies = Array.Empty<LayerProcLiteDependency>();

        public readonly LayerProcLiteLayerId LayerId;
        public readonly LayerProcLiteDependency[] Dependencies;

        public LayerProcLiteLayerDescriptor(LayerProcLiteLayerId layerId, params LayerProcLiteDependency[] dependencies)
        {
            LayerId = layerId;
            Dependencies = dependencies ?? NoDependencies;
        }
    }
}
