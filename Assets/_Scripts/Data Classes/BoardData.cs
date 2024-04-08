
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Data_Classes
{
    public class BoardData
    {
        public Vector3 BoardPosition;
        public int[,] NormalItemIds;
        public Dictionary<Vector2Int,int> UnderlayItemIds;
        public Dictionary<Vector2Int,int> OverlayItemIds;
        public int  BoardSpriteID;
        public BoardData(int boardSpriteID,Vector3 boardPosition,int[,] normalItemIds, Dictionary<Vector2Int,int> underlayItemIds, Dictionary<Vector2Int,int> overlayItemIds)
        {
            BoardSpriteID= boardSpriteID;
            BoardPosition = boardPosition;
            NormalItemIds = normalItemIds;
            UnderlayItemIds = underlayItemIds;
            OverlayItemIds = overlayItemIds;
        }
    }
}