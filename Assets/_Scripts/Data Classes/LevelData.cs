using System;
using Sirenix.Serialization;

namespace DefaultNamespace
{
    [Serializable]
    public class LevelData
    {
        [OdinSerialize]
        public int LevelID { get; set; }
        [OdinSerialize]
        public int BackgroundID { get; set; }
        [OdinSerialize]
        public int[] GoalSpriteIDs { get; set; }
        [OdinSerialize]
        public int[] GoalCounts { get; set; }
        [OdinSerialize]
        public int MoveCount { get; set; }
        [OdinSerialize]
        public int[,] BoardElementIDs { get; set; }
    }
}