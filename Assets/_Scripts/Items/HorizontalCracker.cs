using System;
using System.Collections.Generic;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class HorizontalCracker:ItemBase
    {
        public override void OnClick(IItem[,] board, Vector2Int pos)
        {
            int boardWidth = board.GetLength(0);
            List<Vector3Int> positions = new List<Vector3Int>();
            int index = 0;
            positions.Add(new Vector3Int(pos.x, pos.y, 0));
            index++;
            while (index < boardWidth)
            { 
                if(pos.x+index<boardWidth)
                {
                    positions.Add(new Vector3Int(pos.x + index, pos.y, 0));
                }
                if(pos.x-index>=0)
                {
                    positions.Add(new Vector3Int(pos.x- index, pos.y , 0));
                }
                index++;
            }
            EventManager.Instance.Broadcast(GameEvents.OnHorizontalMatch, positions);
        }
        
        public override void OnMatch()
        {
            base.OnMatch();
            Debug.Log("Horizontal Cracker Matched");
        }
    }
}