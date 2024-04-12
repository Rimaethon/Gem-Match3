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
        public SpawnHandler(Board board)
        {
            _board = board;
            _width = _board.Width;
            _height = _board.Height;
            EventManager.Instance.AddHandler<Vector2Int,int>(GameEvents.AddItemToAddToBoard, AddItemToAddToBoard);
        }
        
        private void AddItemToAddToBoard(Vector2Int itemPos,int itemID)
        {
            _itemsToAddToBoard.Add(itemPos,itemID);
            Debug.Log("Item Added to board"+itemPos);
        }
        public void HandleBoosterSpawn()
        {
            _itemsToAddToBoardThisFrame= new Dictionary<Vector2Int, int>(_itemsToAddToBoard);
            foreach (KeyValuePair<Vector2Int,int> itemData in _itemsToAddToBoardThisFrame)
            {
                _board.GetCell(itemData.Key).SetIsLocked(true);
                _itemsToAddToBoard.Remove(itemData.Key);
                _board.GetCell(itemData.Key).SetItem(ObjectPool.Instance.GetBoosterGameObject(itemData.Value, LevelGrid.Instance.GetCellCenterWorld(itemData.Key), _board));
                _board.GetItem(itemData.Key).Transform.parent = LevelGrid.Grid.transform;
                _board.GetItem(itemData.Key).FallSpeed = 2;

                ObjectPool.Instance.GetBoosterCreationEffect(LevelGrid.Instance.GetCellCenterWorld(itemData.Key));
                _board.GetItem(itemData.Key).Transform.DOPunchScale(Vector3.one * 1.2f, 0.3f,0,0).onComplete += () =>
                {
                    _board.GetCell(itemData.Key).SetIsLocked(false);
                };
                
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
                _board.GetCell(x,_height-1).SetItem(ObjectPool.Instance.GetItemGameObject(randomType, LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(x, _height, 0)),_board));
                _board.GetItem(x, _height - 1).Transform.parent = LevelGrid.Grid.transform;
                _board.GetItem(x, _height - 1).TargetToMove = new Vector2Int(x, _height - 1);
                 _board.GetItem(x,_height-1).IsMoving = true;
                _board.GetItem(x,_height-1).FallSpeed = 2;
            
            }
        }

        
    }
}