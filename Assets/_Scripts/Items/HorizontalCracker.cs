using System;
using System.Collections.Generic;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class HorizontalCracker:ItemBase
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

        public override HashSet<Vector2Int> OnClick(IItem[,] board, Vector2Int pos,bool isTouch=true)
        {
            base.OnClick(board, pos,isTouch);


            int boardWidth = board.GetLength(0);
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

            int index = 0;
            index++;
            while (index < boardWidth)
            { 
                
                if(pos.x+index<boardWidth&&board[pos.x+index,pos.y]!=null)
                {
                    positions.Add(new Vector2Int(pos.x + index, pos.y));
                }
          
                if(pos.x-index>=0&&board[pos.x-index,pos.y]!=null)
                {
                    positions.Add(new Vector2Int(pos.x- index, pos.y ));
                }
                index++;
            }
            if (isTouch)
            {
                EventManager.Instance.Broadcast(GameEvents.OnHorizontalMatch, positions, pos);
            }
            return positions;
        }
        
        public override void OnMatch()
        {
            base.OnMatch();
            Debug.Log("Horizontal Cracker Matched");
        }
    }
}