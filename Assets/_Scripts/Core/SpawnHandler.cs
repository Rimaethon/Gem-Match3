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
        private readonly List<Vector2Int> _spawnCells ;
        public SpawnHandler(GameObject boardInstance,Board board,bool[] dirtyColumns, List<int> spawnAbleFillerItemIds,List<Vector2Int> spawnCells)
        {
            _board = board;
            _width = _board.Width;
            _height = _board.Height;
            _boardInstance = boardInstance;
            _dirtyColumns = dirtyColumns;
            _spawnAbleFillerItemIds = spawnAbleFillerItemIds;
            _spawnCells = spawnCells;
            EventManager.Instance.AddHandler<Vector2Int,int>(GameEvents.AddItemToAddToBoard, AddBoosterToAddToTheBoard);
        }
        public void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int,int>(GameEvents.AddItemToAddToBoard, AddBoosterToAddToTheBoard);
        }

        private void AddBoosterToAddToTheBoard(Vector2Int itemPos,int itemID)
        {
            if(!_itemsToAddToBoard.TryAdd(itemPos, itemID)) return;
        }
        public bool HandleBoosterSpawn()
        {
            bool isThereBoosterToSpawn = false;
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
                isThereBoosterToSpawn = true;
            }
            _itemsToAddToBoard.Clear();
            return isThereBoosterToSpawn;
        }
        public bool HandleFillSpawn()
        {
            bool isAnyCellEmpty = false;

            foreach (var cell in _spawnCells)
            {
                if (_board.Cells[cell.x,cell.y].HasItem||_board.Cells[cell.x,cell.y].IsLocked) continue;
                int randomType = _spawnAbleFillerItemIds[Random.Range(0, _spawnAbleFillerItemIds.Count)];
                _board.Cells[cell.x,cell.y].SetItem(ObjectPool.Instance.GetItem(randomType, LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y+1, 0)),_board));
                _board.GetItem(cell.x, cell.y).Transform.parent = _boardInstance.transform;
                _board.GetItem(cell.x, cell.y).TargetToMove = new Vector2Int(cell.x, cell.y);
                 _board.GetItem(cell.x, cell.y).IsMoving = true;
                 _dirtyColumns[cell.x] = true;
                _board.Cells[cell.x,cell.y].SetIsGettingEmptied(false);
                _board.Cells[cell.x,cell.y].SetIsGettingFilled(true);
                isAnyCellEmpty = true;
            }
            return isAnyCellEmpty;
        }


    }
}
