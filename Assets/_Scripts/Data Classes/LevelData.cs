using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using Sirenix.Serialization;

namespace DefaultNamespace
{
    [Serializable]
    public class LevelData
    {
        [OdinSerialize] 
        public List<BoardData> Boards;
        [OdinSerialize] 
        public GoalSaveData GoalSaveData;

        [OdinSerialize] 
        public int MoveCount;
    }
}