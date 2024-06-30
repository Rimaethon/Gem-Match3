using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class VerticalRocketBoosterAction : IItemAction
    {
        private float _boardDownEdge;
        private float _boardUpEdge;
        private readonly float _rocketOffset = 0.3f;
        private readonly float _speed = 6f;
        private readonly HashSet<Vector2Int> _visitedCells=new HashSet<Vector2Int>();
        private Transform _downRocket;
        private bool _isLockedSetToFalse;
        private Vector2Int _pos;
        private Transform _upRocket;
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            Board = board;
            _pos = pos;
            ItemID = value1;
            _boardUpEdge = Board.Cells.GetBoardBoundaryTopY() + 0.2f;
            _boardDownEdge = Board.Cells.GetBoardBoundaryBottomY() - 0.2f;
            var upRocketPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            var downRocketPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            upRocketPos.y += _rocketOffset;
            downRocketPos.y -= _rocketOffset;
            _upRocket = ObjectPool.Instance.GetBoosterParticleEffect(ItemID, upRocketPos, Quaternion.Euler(0, 0, -90))
                .transform;
            _downRocket = ObjectPool.Instance
                .GetBoosterParticleEffect(ItemID, downRocketPos, Quaternion.Euler(0, 0, 90)).transform;
            Board.Cells[_pos.x,_pos.y].SetIsLocked(true);
            if (Board.Cells[_pos.x,_pos.y].HasItem && !Board.GetItem(_pos).IsExploding)
                Board.GetItem(_pos).OnExplode();
            IsFinished = false;
            _visitedCells.Clear();
        }

        public void Execute()
        {
            if (_upRocket.transform.position.y < 5f || _downRocket.transform.position.y > -5f)
            {
                _upRocket.transform.position += Vector3.up * (_speed * Time.fixedDeltaTime);
                _downRocket.transform.position += Vector3.down * (_speed * Time.fixedDeltaTime);
                if (_upRocket.transform.position.y < _boardUpEdge || _downRocket.transform.position.y > _boardDownEdge)
                {
                    var upRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_upRocket.position);
                    var downRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_downRocket.position);
                    HandleExplosion(upRocketCell);
                    HandleExplosion(downRocketCell);
                }

                return;
            }
            ObjectPool.Instance.ReturnBoosterParticleEffect(_upRocket.gameObject, ItemID);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_downRocket.gameObject, ItemID);
            IsFinished = true;
            SetColumnLock(false);


        }

        private void HandleExplosion(Vector2Int cell)
        {
            if (!Board.IsInBoundaries(cell)) return;
            if(_visitedCells.Contains(cell)) return;
            if (Board.Cells[cell.x, cell.y].HasItem )
            {
                if (!Board.GetItem(cell).IsGeneratorItem && !Board.GetItem(cell).IsExploding)
                {
                    Board.GetItem(cell).OnExplode();
                }

            }
            else
            {
                EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, cell);
            }
            Board.Cells[cell.x,cell.y].SetIsLocked(true);
            _visitedCells.Add(cell);
        }

        private void SetColumnLock(bool isLocked)
        {
            for (var i = 0; i < Board.Height; i++) Board.Cells[_pos.x,i].SetIsLocked(isLocked);
        }
    }
}
