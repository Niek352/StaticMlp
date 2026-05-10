using System;
using UnityEngine;

namespace StaticMlp.Features.World
{
    [Serializable]
    public struct Stage1BotSpawnSeed
    {
        public Vector3 Position;
        public ushort BehaviorId;
        public float Health01;
        public float Hunger;
        public float Fear;
        public int LeaderIndex;
    }
}
