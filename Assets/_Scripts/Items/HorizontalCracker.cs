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
        }

        public override void OnClick(IItem[,] board, Vector2Int pos)
        {
            int boardWidth = board.GetLength(0);
            List<Vector2Int> positions = new List<Vector2Int>();
            int index = 0;
            positions.Add(new Vector2Int(pos.x, pos.y));
            index++;
            while (index < boardWidth)
            { 
                if(pos.x+index<boardWidth)
                {
                    positions.Add(new Vector2Int(pos.x + index, pos.y));
                }
                if(pos.x-index>=0)
                {
                    positions.Add(new Vector2Int(pos.x- index, pos.y ));
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