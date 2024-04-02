
using UnityEngine;

namespace Scripts
{
    public class Cell
    {
        public Cell(Vector2Int cellPosition,IItem item=null,IItem underLayItem=null,IItem overLayItem=null,bool isNotInBoard=false)
        {
            CellPosition = cellPosition;
            SetItem(item);
            UnderLayItem = underLayItem;
            OverLayItem = overLayItem;
            IsNotInBoard = isNotInBoard;
        }
        public bool IsNotInBoard;
        public bool HasItem => Item != null;
        public bool HasUnderLayItem => UnderLayItem != null;
        public bool HasOverLayItem => OverLayItem != null;
        public Vector2Int CellPosition;
        public bool IsGettingFilled => _isGettingFilled;
        public bool IsGettingEmptied => _isGettingEmptied;
        public bool IsLocked => _isLocked;
        public IItem Item=>_item;
        private IItem _item;
        public IItem UnderLayItem;
        public IItem OverLayItem;
        private bool _isLocked;
        private bool _isGettingFilled ;
        private bool _isGettingEmptied;
        
        public void SetItem(IItem item)
        {
            _item = item;
            if (Item != null)
            {
                Item.Position = CellPosition;
            }
          Debug.Log("Item set to "+item+" for cell "+CellPosition.x+" "+CellPosition.y);
        }
        
        public void SetIsGettingFilled(bool value)
        {
            _isGettingFilled = value;
           Debug.Log("Is getting filled set to "+value+" for cell "+CellPosition.x+" "+CellPosition.y);
        }
        public void SetIsGettingEmptied(bool value)
        {
            _isGettingEmptied = value;
       Debug.Log("Is getting Emptied set to "+value+" for cell "+CellPosition.x+" "+CellPosition.y);
        }
        public void SetIsLocked(bool value)
        {
            _isLocked = value;
       Debug.Log("Is getting Locked set to "+value+" for cell "+CellPosition.x+" "+CellPosition.y);
        }
    }
}