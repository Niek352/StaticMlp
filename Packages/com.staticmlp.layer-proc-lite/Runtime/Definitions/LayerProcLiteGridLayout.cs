using System;

namespace StaticMlp.LayerProcLite
{
    public readonly struct LayerProcLiteGridLayout
    {
        public readonly int OutputResolution;
        public readonly int PaddingSamples;
        public readonly int InputResolution;

        public LayerProcLiteGridLayout(int outputResolution, int paddingSamples)
        {
            if (outputResolution <= 1)
                throw new ArgumentOutOfRangeException(nameof(outputResolution), outputResolution, "Output resolution must be greater than one.");
            if (paddingSamples < 0)
                throw new ArgumentOutOfRangeException(nameof(paddingSamples), paddingSamples, "Padding samples must be non-negative.");

            OutputResolution = outputResolution;
            PaddingSamples = paddingSamples;
            InputResolution = outputResolution + paddingSamples * 2;
        }

        public int OutputSampleCount => OutputResolution * OutputResolution;
        public int InputSampleCount => InputResolution * InputResolution;

        public int ToOutputIndex(int outputX, int outputZ)
        {
            return LayerProcLiteGrid.ToIndex(outputX, outputZ, OutputResolution);
        }

        public int ToInputIndex(int outputX, int outputZ)
        {
            return LayerProcLiteGrid.ToIndex(outputX + PaddingSamples, outputZ + PaddingSamples, InputResolution);
        }

        public int ToInputIndexRaw(int inputX, int inputZ)
        {
            return LayerProcLiteGrid.ToIndex(inputX, inputZ, InputResolution);
        }
    }
}
