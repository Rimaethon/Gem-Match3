using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class Bomb:ItemBase
    {

        public override void OnClick(IItem[,] board, Vector2Int pos)
        {
            int boardWidth = board.GetLength(0);
            int boardHeight = board.GetLength(1);
            List<Vector2Int> positions = new List<Vector2Int>();
            positions.Add(new Vector2Int(pos.x, pos.y));
            for (int x = pos.x - 2; x <= pos.x + 2; x++)
            {
                for (int y = pos.y - 2; y <= pos.y + 2; y++)
                {
                    if (x >= 0 && x < boardWidth && y >= 0 && y < boardHeight&&board[x,y]!=null&&(x!=pos.x||y!=pos.y))
                    {
                        
                        positions.Add(new Vector2Int(x, y));
                    }
                 
                }
            }
            EventManager.Instance.Broadcast(GameEvents.OnBomb, positions);
        }
        public override void OnMatch()
        {
            base.OnMatch();
            Debug.Log("Bomb Matched");
        }
     
    }
}