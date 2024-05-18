using UnityEngine;

namespace Scripts
{
    public class Cell
    {
        public Cell(Vector2Int cellPosition,IBoardItem boardItem=null,IBoardItem underLayBoardItem=null,IBoardItem overLayBoardItem=null,bool isNotInBoard=false)
        {
            CellPosition = cellPosition;
            SetItem(boardItem);
            _underLayBoardItem = underLayBoardItem;
            _overLayBoardItem= overLayBoardItem;
            IsNotInBoard = isNotInBoard;
        }
        public bool IsNotInBoard;
        public bool HasItem => BoardItem != null;
        public bool HasUnderLayItem => UnderLayBoardItem != null;
        public bool HasOverLayItem => OverLayBoardItem != null;
        public Vector2Int CellPosition;
        public bool IsGettingFilled => _isGettingFilled;
        public bool IsGettingEmptied => _isGettingEmptied;
        public bool IsLocked => _isLocked;
        public IBoardItem BoardItem=>_boardItem;
        private IBoardItem _boardItem;
        public IBoardItem UnderLayBoardItem=>_underLayBoardItem;
        private IBoardItem _underLayBoardItem;
        public IBoardItem OverLayBoardItem=>_overLayBoardItem;
        private IBoardItem _overLayBoardItem;
        private bool _isLocked;
        private bool _isGettingFilled ;
        private bool _isGettingEmptied;
        private int _lockCount=0;
        public void SetItem(IBoardItem boardItem)
        {
            _boardItem = boardItem;
            if (BoardItem != null)
            {
                BoardItem.Position = CellPosition;
            } 
        }
        public void SetUnderLayItem(IBoardItem boardItem)
        {
            _underLayBoardItem = boardItem;
            if (UnderLayBoardItem != null)
            {
                UnderLayBoardItem.Position = CellPosition;
            }
        }
        public void SetOverLayItem(IBoardItem boardItem)
        {
            _overLayBoardItem = boardItem;
            if (OverLayBoardItem != null)
            {
                OverLayBoardItem.Position = CellPosition;
            }
        }
        
        public void SetIsGettingFilled(bool value)
        {
            _isGettingFilled = value;
        }
        public void SetIsGettingEmptied(bool value)
        {
            _isGettingEmptied = value;
        }
        public void SetIsLocked(bool value)
        {
            _lockCount=value?_lockCount+1:_lockCount-1;
            if (_lockCount <= 0)
            {
                _isLocked = false;
                
            }else
            {
                _isLocked = true;
            }
        }
    }
}