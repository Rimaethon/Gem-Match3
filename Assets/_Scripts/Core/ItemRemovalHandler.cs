using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Core
{
    //This class is for removing item references from cells.
    //Since there is a delay between the explosion visual and the actual removal of the item from the board, it needs to be handled separately.
    public class ItemRemovalHandler
    {
        private readonly HashSet<Vector2Int> _itemsToRemoveFromBoard = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> _itemsToRemoveFromBoardThisFrame = new HashSet<Vector2Int>();
        private Board _board;
        public ItemRemovalHandler (Board board)
        {
            _board = board;
            EventManager.Instance.AddHandler<Vector2Int>(GameEvents.AddItemToRemoveFromBoard, AddItemToRemoveFromBoard);
        }
       
        
        private void AddItemToRemoveFromBoard(Vector2Int itemPos)
        {
            _itemsToRemoveFromBoard.Add(itemPos);
            
        }

        public void HandleItemRemoval()
        {
            _itemsToRemoveFromBoardThisFrame.UnionWith(_itemsToRemoveFromBoard);
            
            foreach (Vector2Int itemPos in _itemsToRemoveFromBoardThisFrame)
            {
                _itemsToRemoveFromBoard.Remove(itemPos);
//                Debug.Log("Item Removed from board"+itemPos.x+" "+itemPos.y);
                Cell cell = _board.GetCell(itemPos);
                cell.SetIsGettingEmptied(false);
                cell.SetIsGettingFilled(false);

                if (cell.HasItem)
                {
                    if (cell.Item.IsMoving)
                    {
                        _board.GetCell(cell.Item.TargetToMove).SetIsGettingFilled(false);
                        cell.Item.IsMoving = false;
                    }
                }
                Debug.Log("Item Removed from board"+itemPos.x+" "+itemPos.y+" "+cell.HasUnderLayItem);
                if (cell.HasUnderLayItem)
                {
                    cell.UnderLayItem.OnExplode();
                }
                cell.SetItem(null);
            }
            _itemsToRemoveFromBoardThisFrame.Clear();

        }
    }
}