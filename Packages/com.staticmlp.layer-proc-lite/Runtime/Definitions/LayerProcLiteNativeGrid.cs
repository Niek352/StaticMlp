using System;
using Unity.Collections;

namespace StaticMlp.LayerProcLite
{
    public struct LayerProcLiteNativeGrid<T>
        where T : unmanaged
    {
        public NativeArray<T> Values;
        public LayerProcLiteGridLayout Layout;

        public LayerProcLiteNativeGrid(NativeArray<T> values, LayerProcLiteGridLayout layout)
        {
            if (!values.IsCreated)
                throw new InvalidOperationException("Native grid values must be created.");
            if (values.Length != layout.InputSampleCount)
                throw new ArgumentException("Native grid values length must match the grid input sample count.", nameof(values));

            Values = values;
            Layout = layout;
        }

        public T GetInput(int inputX, int inputZ)
        {
            return Values[Layout.ToInputIndexRaw(inputX, inputZ)];
        }

        public void SetInput(int inputX, int inputZ, T value)
        {
            Values[Layout.ToInputIndexRaw(inputX, inputZ)] = value;
        }

        public T GetOutput(int outputX, int outputZ)
        {
            return Values[Layout.ToInputIndex(outputX, outputZ)];
        }

        public void SetOutput(int outputX, int outputZ, T value)
        {
            Values[Layout.ToInputIndex(outputX, outputZ)] = value;
        }
    }
}
