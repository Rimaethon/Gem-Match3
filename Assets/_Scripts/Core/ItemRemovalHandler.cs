using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Core
{
    //This class is for removing item references from cells.
    //Since there is a delay between the explosion visual and the actual removal of the item from the board, it needs to be handled separately.
    public class ItemRemovalHandler
    {
        private readonly HashSet<Vector2Int> _itemsToRemoveFromBoard = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> _itemsToRemoveFromBoardThisFrame = new List<Vector2Int>();
        private Board _board;
        private bool[] _dirtyColumns;
        public ItemRemovalHandler (Board board,bool[] dirtyColumns)
        {
            _board = board;
            _dirtyColumns = dirtyColumns;
            EventManager.Instance.AddHandler<Vector2Int>(GameEvents.AddItemToRemoveFromBoard, AddItemToRemoveFromBoard);
        }
       
        public void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int>(GameEvents.AddItemToRemoveFromBoard, AddItemToRemoveFromBoard);
        }
        private void AddItemToRemoveFromBoard(Vector2Int itemPos)
        {
            _itemsToRemoveFromBoard.Add(itemPos);
            
        }

        public bool HandleItemRemoval()
        {
            _itemsToRemoveFromBoardThisFrame.AddRange(_itemsToRemoveFromBoard);
            
            foreach (Vector2Int itemPos in _itemsToRemoveFromBoardThisFrame)
            {
                _itemsToRemoveFromBoard.Remove(itemPos);
//                Debug.Log("Item Removed from board"+itemPos.x+" "+itemPos.y);
                Cell cell = _board.Cells[itemPos.x,itemPos.y];
                cell.SetIsGettingEmptied(false);
                cell.SetIsGettingFilled(false);
                _dirtyColumns[itemPos.x] = true;
                if (cell.HasOverLayItem)
                {
                    cell.OverLayBoardItem.OnExplode();
                    if(cell.OverLayBoardItem.IsProtectingUnderIt)
                        continue;
                }
                if (cell.HasItem)
                {
                    if (cell.BoardItem.IsMoving)
                    {
                        _board.Cells[cell.BoardItem.TargetToMove.x,cell.BoardItem.TargetToMove.y].SetIsGettingFilled(false);
                        cell.BoardItem.IsMoving = false;
                    }
                    ObjectPool.Instance.ReturnItem(cell.BoardItem, cell.BoardItem.ItemID);
                    if(cell.BoardItem.IsProtectingUnderIt)
                        continue;
                }
                if (cell.HasUnderLayItem)
                {
                    cell.UnderLayBoardItem.OnExplode();
                }
                cell.SetItem(null);
            }
            _itemsToRemoveFromBoardThisFrame.Clear();
            return _itemsToRemoveFromBoard.Count > 0;
        }
    }
}