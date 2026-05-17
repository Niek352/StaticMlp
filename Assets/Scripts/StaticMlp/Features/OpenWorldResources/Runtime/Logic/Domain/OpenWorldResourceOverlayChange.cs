namespace StaticMlp.Features.OpenWorldResources
{
    public readonly struct OpenWorldResourceOverlayChange
    {
        public readonly uint BasisRevision;
        public readonly uint Revision;
        public readonly OpenWorldResourceOverlayDelta Delta;

        public OpenWorldResourceOverlayChange(
            uint basisRevision,
            uint revision,
            OpenWorldResourceOverlayDelta delta)
        {
            BasisRevision = basisRevision;
            Revision = revision;
            Delta = delta;
        }
    }
}
