using System.Collections.Generic;
using _Scripts.Utility;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Core
{
    public class SpawnHandler
    {
        
        private Dictionary<Vector2Int,int> _itemsToAddToBoard = new Dictionary<Vector2Int,int> ();
        private Dictionary<Vector2Int,int>  _itemsToAddToBoardThisFrame = new Dictionary<Vector2Int,int> ();
        private readonly Board _board;
        private readonly int _width;
        private readonly int _height;
        private GameObject _boardInstance;
        public SpawnHandler(GameObject boardInstance,Board board)
        {
            _board = board;
            _width = _board.Width;
            _height = _board.Height;
            _boardInstance = boardInstance;
            EventManager.Instance.AddHandler<Vector2Int,int>(GameEvents.AddItemToAddToBoard, AddBoosterToAddToTheBoard);

        }
        
        //I hope someday I don't be so brain dead  and give better names to these methods
        private void AddBoosterToAddToTheBoard(Vector2Int itemPos,int itemID)
        {
            if(!_itemsToAddToBoard.TryAdd(itemPos, itemID)) return;
        }
        public void HandleBoosterSpawn()
        {
            _itemsToAddToBoardThisFrame= new Dictionary<Vector2Int, int>(_itemsToAddToBoard);
            foreach (KeyValuePair<Vector2Int,int> itemData in _itemsToAddToBoardThisFrame)
            {
                _board.GetCell(itemData.Key).SetIsLocked(true);
                _itemsToAddToBoard.Remove(itemData.Key);
                
                IBoardItem boardItem = ObjectPool.Instance.GetBoosterItem(itemData.Value, LevelGrid.Instance.GetCellCenterWorld(itemData.Key), _board);
                boardItem.Transform.parent = _boardInstance.transform;
                if (_board.GetCell(itemData.Key).HasItem)
                {
                    ObjectPool.Instance.ReturnItem(_board.GetItem(itemData.Key), _board.GetItem(itemData.Key).ItemID);
                }
                _board.GetCell(itemData.Key).SetIsGettingEmptied(false);
                _board.GetCell(itemData.Key).SetIsGettingFilled(false);
                _board.GetCell(itemData.Key).SetItem(boardItem);
                _board.GetItem(itemData.Key).TargetToMove = itemData.Key;
                _board.GetItem(itemData.Key).IsMoving = true;
                _board.GetCell(itemData.Key).SetIsLocked(false);
                boardItem.Transform.DOScale(Vector3.one * 1.2f, 0.15f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetUpdate(UpdateType.Fixed);

            }
            _itemsToAddToBoardThisFrame.Clear();
        }
        public void HandleFillSpawn()
        {
            for (int x = 0; x < _width; x++)
            {
                //           Debug.Log("Checking spawn"+x+" "+_height+" "+_board.GetCell(x,_height-1).HasItem+" "+_board.GetCell(x,_height-1).IsLocked);
                if (_board.GetCell(x, _height - 1).HasItem||_board.GetCell(x,_height-1).IsLocked) continue;
                int randomType = Random.Range(0, 4);
                _board.GetCell(x,_height-1).SetItem(ObjectPool.Instance.GetItem(randomType, LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(x, _height, 0)),_board));
                _board.GetItem(x, _height - 1).Transform.parent = _boardInstance.transform;
                _board.GetItem(x, _height - 1).TargetToMove = new Vector2Int(x, _height - 1);
                 _board.GetItem(x,_height-1).IsMoving = true;
                _board.GetCell(x,_height-1).SetIsGettingEmptied(false);
                _board.GetCell(x,_height-1).SetIsGettingFilled(false);
            
            }
        }

        
    }
}