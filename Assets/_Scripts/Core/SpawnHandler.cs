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
        private readonly Board _board;
        private readonly int _width;
        private readonly int _height;
        private GameObject _boardInstance;
        private bool[] _dirtyColumns;
        private readonly List<int> _spawnAbleFillerItemIds;
        public SpawnHandler(GameObject boardInstance,Board board,bool[] dirtyColumns, List<int> spawnAbleFillerItemIds)
        {
            _board = board;
            _width = _board.Width;
            _height = _board.Height;
            _boardInstance = boardInstance;
            _dirtyColumns = dirtyColumns;
            _spawnAbleFillerItemIds = spawnAbleFillerItemIds;
            EventManager.Instance.AddHandler<Vector2Int,int>(GameEvents.AddItemToAddToBoard, AddBoosterToAddToTheBoard);
        }
        public void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int,int>(GameEvents.AddItemToAddToBoard, AddBoosterToAddToTheBoard);
        }
        
        //I hope someday I don't be so brain dead  and give better names to these methods
        private void AddBoosterToAddToTheBoard(Vector2Int itemPos,int itemID)
        {
            if(!_itemsToAddToBoard.TryAdd(itemPos, itemID)) return;
        }
        public void HandleBoosterSpawn()
        {
            foreach (KeyValuePair<Vector2Int,int> itemData in _itemsToAddToBoard)
            {
                _board.Cells[itemData.Key.x,itemData.Key.y].SetIsLocked(true);
                Vector3 pos = LevelGrid.Instance.GetCellCenterWorld(itemData.Key);
                IBoardItem boardItem = ObjectPool.Instance.GetBoosterItem(itemData.Value, pos, _board);
                boardItem.Transform.parent = _boardInstance.transform;
                boardItem.Transform.position = pos;
                if (_board.Cells[itemData.Key.x,itemData.Key.y].HasItem)
                {
                    ObjectPool.Instance.ReturnItem(_board.GetItem(itemData.Key), _board.GetItem(itemData.Key).ItemID);
                }
                _board.Cells[itemData.Key.x,itemData.Key.y].SetIsGettingEmptied(false);
                _board.Cells[itemData.Key.x,itemData.Key.y].SetIsGettingFilled(false);
                _board.Cells[itemData.Key.x,itemData.Key.y].SetItem(boardItem);
                _board.GetItem(itemData.Key).TargetToMove = itemData.Key;
                _dirtyColumns[itemData.Key.x] = true;
                _board.Cells[itemData.Key.x,itemData.Key.y].SetIsLocked(false);
                boardItem.Transform.DOScale(Vector3.one * 1.4f, 0.15f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetUpdate(UpdateType.Fixed);
            }
            _itemsToAddToBoard.Clear();
        }
        public bool HandleFillSpawn()
        {
            bool isAnyCellEmpty = false;
            for (int x = 0; x < _width; x++)
            {
                //           Debug.Log("Checking spawn"+x+" "+_height+" "+_board.GetCell(x,_height-1).HasItem+" "+_board.GetCell(x,_height-1).IsLocked);
                if (_board.Cells[x,_height-1].HasItem||_board.Cells[x,_height-1].IsLocked) continue;
                int randomType = _spawnAbleFillerItemIds[Random.Range(0, _spawnAbleFillerItemIds.Count)];
                _board.Cells[x,_height-1].SetItem(ObjectPool.Instance.GetItem(randomType, LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(x, _height, 0)),_board));
                _board.GetItem(x, _height - 1).Transform.parent = _boardInstance.transform;
                _board.GetItem(x, _height - 1).TargetToMove = new Vector2Int(x, _height - 1);
                 _board.GetItem(x,_height-1).IsMoving = true;
                _board.Cells[x,_height-1].SetIsGettingEmptied(false);
                _board.Cells[x,_height-1].SetIsGettingFilled(false);
                isAnyCellEmpty = true;
            
            }
            return isAnyCellEmpty;
        }

        
    }
}