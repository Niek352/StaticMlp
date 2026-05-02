namespace StaticMlp.Game.Bootstrap {
    public static class GameplaySystemOrder {
        public const short ServerConnectionGameplay = -830;
        public const short ClientApplyNetworkState = -800;
        public const short Gameplay = 0;
        public const short ClientPresentation = 250;
        public const short CollectReplication = 500;
    }
}
