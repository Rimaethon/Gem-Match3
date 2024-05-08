using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using Sirenix.Serialization;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable]
    public class LevelData
    {
        //Shhh Secret Experiments are happening here
        public readonly bool isCastleLevel;
        public readonly CastleData CastleData;
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

    public class CastleData
    {
        Vector2Int[] _itemPositions;
        int[] _itemIds;
        Vector3 _levelPositions;
    }
}