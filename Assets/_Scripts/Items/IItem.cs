using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public interface IItem
    {
        public bool IsActive 
        { 
            get;
            set; 
        }
        public int ItemID
        {
            get;
            set;
        }
        public float FallSpeed
        {
            get;
            set;
        }
        public bool IsFallAble 
        { 
            get;
            set;
        }
        public bool IsMatchable
        {
            get;
            set;
        }
  
        public bool IsSwappable
        {
            get;
            set;
        }
        public bool IsSwapping
        {
            get;
            set;
        }
        public Vector2Int SwappingFrom
        {
            get;
            set;
        }
        public Vector2Int TargetToMove 
        { 
            get;
            set;
        }
        public Vector2Int Position
        {
            get;
            set;
        }
        public float Gravity
        {
            get;
        }
        public bool IsBooster
        {
            get;
        }
        public bool IsMoving
        {
            get;
            set;
        }
        public bool IsHighlightAble
        {
            get;
        }
        public Board Board
        {
            get;
            set;
        }
        public bool IsExploding
        {
            get;
        }
        public bool IsMatching
        {
            get;
            set;
        }
        public void Highlight(float value);
        public abstract void SetSortingOrder(int order);
        public Transform Transform { get; }
        public void OnMatch();
        public void OnExplode();
        public void OnRemove();
        public void OnTouch();
        public void OnClick(Board board,Vector2Int pos);
        public void OnSwap(IItem item,IItem otherItem);

    }

}