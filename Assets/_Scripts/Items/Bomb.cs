using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class Bomb:ItemBase
    {

        public override HashSet<Vector2Int> OnClick(IItem[,] board, Vector2Int pos,bool isTouch)
        {
            base.OnClick(board, pos,isTouch);
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
            int boardWidth = board.GetLength(0);
            int boardHeight = board.GetLength(1);
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

            if (isTouch)
            {
                EventManager.Instance.Broadcast(GameEvents.OnBomb, positions, pos);
            }
            return positions;
        }
        public override void OnMatch()
        {
            base.OnMatch();
            Debug.Log("Bomb Matched");
        }
     
    }
}