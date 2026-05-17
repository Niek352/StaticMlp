using System;
using Unity.Collections;

namespace StaticMlp.LayerProcLite
{
    public struct LayerProcLiteNativeBuffer<T>
        where T : unmanaged
    {
        public NativeArray<T> Values;

        public LayerProcLiteNativeBuffer(NativeArray<T> values)
        {
            if (!values.IsCreated)
                throw new InvalidOperationException("Native buffer values must be created.");

            Values = values;
        }

        public int Length => Values.Length;
    }
}
