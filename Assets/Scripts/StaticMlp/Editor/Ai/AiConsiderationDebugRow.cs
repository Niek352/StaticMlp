using StaticMlp.Features.AiBots;

namespace StaticMlp.Editor.Ai
{
    public sealed class AiConsiderationDebugRow
    {
        public ushort VariableId { get; set; }
        public string VariableName { get; set; }
        public UtilityCurveType Curve { get; set; }
        public float RawValue { get; set; }
        public float CurvedValue { get; set; }
        public float Weight { get; set; }
        public float Factor { get; set; }
    }
}
