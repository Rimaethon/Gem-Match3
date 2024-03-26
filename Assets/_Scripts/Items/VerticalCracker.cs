using System.Collections.Generic;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class VerticalCracker:ItemBase
    {
        public override void OnClick(IItem[,] board, Vector2Int pos)
        {
            int boardHeight = board.GetLength(1);
            List<Vector3Int> positions = new List<Vector3Int>();
            int index = 0;
            positions.Add(new Vector3Int(pos.x, pos.y, 0));
            index++;
            while (index < boardHeight)
            { 
                if(pos.y+index<boardHeight)
                {
                    positions.Add(new Vector3Int(pos.x, pos.y + index, 0));
                }
                if(pos.y-index>=0)
                {
                    positions.Add(new Vector3Int(pos.x, pos.y - index, 0));
                }
                index++;
            }
            EventManager.Instance.Broadcast(GameEvents.OnHorizontalMatch, positions);
        }
        public override void OnMatch()
        {
            base.OnMatch();
            Debug.Log("Vertical Cracker Matched");
        }
    }
}