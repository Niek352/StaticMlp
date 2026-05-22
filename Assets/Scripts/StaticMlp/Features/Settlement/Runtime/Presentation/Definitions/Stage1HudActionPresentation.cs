namespace StaticMlp.Features.Settlement
{
    public readonly struct Stage1HudActionPresentation
    {
        public readonly string Label;
        public readonly bool Enabled;
        public readonly string DisabledReason;
        public readonly string EffectDescription;

        public Stage1HudActionPresentation(
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
