using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;

namespace DefaultNamespace
{
    [Serializable]
    public class LevelData
    {
        public readonly List<BoardData> Boards;
        public readonly List<int> SpawnAbleFillerItemIds;
        public readonly GoalSaveData GoalSaveData;
        public readonly int backgroundID;
        public readonly int MoveCount;

        public LevelData(List<BoardData> boards, List<int> spawnAbleFillerItemIds, GoalSaveData goalSaveData, int backgroundID, int moveCount)
        {
            Boards = boards;
            SpawnAbleFillerItemIds = spawnAbleFillerItemIds;
            GoalSaveData = goalSaveData;
            this.backgroundID = backgroundID;
            MoveCount = moveCount;
        }
    }
}
