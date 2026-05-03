namespace StaticMlp.Game.Systems
{
    public readonly struct SpawnPhysicsCubeRequestEvent
    {
        public readonly float CameraYaw;

        public SpawnPhysicsCubeRequestEvent(float cameraYaw)
        {
            CameraYaw = cameraYaw;
        }
    }
}
