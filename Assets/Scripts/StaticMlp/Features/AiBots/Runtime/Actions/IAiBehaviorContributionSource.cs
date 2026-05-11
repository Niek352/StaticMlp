namespace StaticMlp.Features.AiBots
{
    public interface IAiBehaviorContributionSource
    {
        void Register(AiBehaviorTaskRegistry registry);
    }
}
