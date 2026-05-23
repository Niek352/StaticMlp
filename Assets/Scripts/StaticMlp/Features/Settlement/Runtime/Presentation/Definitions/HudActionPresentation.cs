namespace StaticMlp.Features.Settlement
{
    public readonly struct HudActionPresentation
    {
        public readonly string Label;
        public readonly bool Enabled;
        public readonly string DisabledReason;
        public readonly string EffectDescription;

        public HudActionPresentation(
            string label,
            bool enabled,
            string disabledReason,
            string effectDescription)
        {
            Label = label;
            Enabled = enabled;
            DisabledReason = disabledReason;
            EffectDescription = effectDescription;
        }
    }
}
