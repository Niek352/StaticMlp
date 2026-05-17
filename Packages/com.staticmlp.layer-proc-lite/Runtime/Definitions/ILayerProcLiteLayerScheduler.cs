namespace StaticMlp.LayerProcLite
{
    public interface ILayerProcLiteLayerScheduler
    {
        LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context);
    }
}
