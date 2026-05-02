namespace StaticMlp.Networking.Replication
{
    public static partial class ReplicationRegistry
    {
        public static void ApplyInitialState(CW.Entity e, System.Collections.Generic.List<ComponentDelta> components)
        {
            foreach (var delta in components)
                ApplyDelta(e, delta);
        }
    }
}