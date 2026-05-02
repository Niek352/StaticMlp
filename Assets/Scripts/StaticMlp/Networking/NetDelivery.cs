namespace StaticMlp.Networking {
    public enum NetDelivery : byte {
        Unreliable = 0,
        UnreliableSequenced = 1,
        ReliableSequenced = 2
    }
}
