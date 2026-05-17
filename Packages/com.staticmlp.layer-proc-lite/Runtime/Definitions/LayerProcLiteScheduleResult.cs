using System;
using Unity.Jobs;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteScheduleResult
    {
        public readonly JobHandle Handle;
        public readonly ILayerProcLiteChunkData Data;

        public LayerProcLiteScheduleResult(JobHandle handle, ILayerProcLiteChunkData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Handle = handle;
        }
    }
}
