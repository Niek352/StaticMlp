namespace StaticMlp.LayerProcLite
{
    public enum LayerProcLiteChunkState : byte
    {
        Unloaded = 0,
        Scheduled = 1,
        Ready = 2,
        Failed = 3
    }
}
