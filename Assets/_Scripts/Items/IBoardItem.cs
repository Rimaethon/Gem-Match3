using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts
{
    //I'm not monolithic, my font size is big.
    public interface IBoardItem:IItem
    {
        public bool IsActive 
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
        }
        public bool IsMatchable
        {
            get;
        }
        public bool IsSwappable
        {
            get;
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

        public bool IsExplodeAbleByNearMatches
        {
            get;
        }
        public bool IsGeneratorItem
        {
            get;
        }
        public bool IsShuffleAble
        {
            get;
        }
        //This is for checking if item is blocking under it such as a stone prevents bush from exploding.
        public bool IsProtectingUnderIt
        {
            get;
        }
        public void Highlight(float value);
        public abstract void SetSortingOrder(int order);
        public Transform Transform { get; }
        public void OnMatch();
        public void OnExplode();
        public void OnRemove();
        public void OnTouch();
        public void OnClick(Board board,Vector2Int pos);
        public void OnSwap(IBoardItem boardItem,IBoardItem otherBoardItem);
    }

 
}