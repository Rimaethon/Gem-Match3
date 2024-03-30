using System.Collections.Generic;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class VerticalCracker:ItemBase
    {
        [SerializeField] private SpriteRenderer leftCracker;
        [SerializeField] private SpriteRenderer rightCracker;

        public override int SortingOrder
        {
            get=> leftCracker.sortingOrder; 
            set
            {
                if (leftCracker != null)
                {
                    leftCracker.sortingOrder = value;
                }
                if (rightCracker != null)
                {
                    rightCracker.sortingOrder = value;
                }
            }
        }

        public override void OnTouch()
        {
            if (leftCracker != null&&rightCracker!=null)
            {
                leftCracker.color = leftCracker.color==_touchedColor?Color.white:_touchedColor;
                rightCracker.color = rightCracker.color==_touchedColor?Color.white:_touchedColor;
            }
       
        }
        protected override void Awake()
        {
            if (leftCracker != null&&rightCracker!=null)
            {
                leftCracker.color = _touchedColor;
                rightCracker.color = _touchedColor;
            }
            base.Awake();
        }

        public override HashSet<Vector2Int> OnClick(IItem[,] board, Vector2Int pos,bool isTouch)
        {
            base.OnClick(board, pos,isTouch);
            int boardHeight = board.GetLength(1);
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
            int index = 0;
            index++;
            while (index < boardHeight)
            { 
                if(pos.y+index<boardHeight&&board[pos.x,pos.y+index]!=null)
                {
                    positions.Add(new Vector2Int(pos.x, pos.y + index));
                }
           
                if(pos.y-index>=0&&board[pos.x,pos.y-index]!=null)
                {
                    positions.Add(new Vector2Int(pos.x, pos.y - index));
                }
       
                index++;
            }
            if (isTouch)
            {
                EventManager.Instance.Broadcast(GameEvents.OnVerticalMatch, positions, pos);
            }
            return positions;
        }
        
     
    }
}