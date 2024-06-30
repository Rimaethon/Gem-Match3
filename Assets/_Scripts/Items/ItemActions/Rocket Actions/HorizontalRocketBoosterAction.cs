using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class HorizontalRocketBoosterAction : IItemAction
    {
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public bool IsFinished { get; set; }

        private float _boardLeftEdge;
        private float _boardRightEdge;
        private readonly float _rocketOffset = 0.1f;
        private readonly float _speed = 7f;
        private readonly HashSet<Vector2Int> _visitedCells=new HashSet<Vector2Int>();
        private bool _isLockedSetToFalse;
        private Transform _leftRocket;
        private Vector2Int _pos;
        private Transform _rightRocket;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            _pos = pos;
            Board = board;
            ItemID = value1;
            _boardRightEdge = Board.Cells.GetBoardBoundaryRightX() + 0.2f;
            _boardLeftEdge = Board.Cells.GetBoardBoundaryLeftX() - 0.2f;
            var leftRocketPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            var rightRocketPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            leftRocketPos.x -= _rocketOffset;
            rightRocketPos.x += _rocketOffset;
            _leftRocket = ObjectPool.Instance.GetBoosterParticleEffect(ItemID, leftRocketPos).transform;
            _rightRocket = ObjectPool.Instance
                .GetBoosterParticleEffect(ItemID, rightRocketPos, Quaternion.Euler(0, 0, 180)).transform;
            SetRowLock(true);
            if (Board.Cells[_pos.x,_pos.y].HasItem && !Board.GetItem(_pos).IsExploding)
                Board.GetItem(_pos).OnExplode();
            IsFinished = false;
            _visitedCells.Clear();
        }

        public void Execute()
        {
            if (_leftRocket.transform.position.x > -3.5f || _rightRocket.transform.position.x < 3.5f)
            {
                _leftRocket.transform.position += Vector3.left * (_speed * Time.fixedDeltaTime);
                _rightRocket.transform.position += Vector3.right * (_speed * Time.fixedDeltaTime);
                if (_leftRocket.transform.position.x > _boardLeftEdge ||
                    _rightRocket.transform.position.x < _boardRightEdge)
                {
                    var leftRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_leftRocket.position);
                    var rightRocketCell = LevelGrid.Instance.WorldToCellVector2Int(_rightRocket.position);
                    HandleExplosion(leftRocketCell);
                    HandleExplosion(rightRocketCell);
                }

                return;
            }
            ObjectPool.Instance.ReturnBoosterParticleEffect(_leftRocket.gameObject, ItemID);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_rightRocket.gameObject, ItemID);
            IsFinished = true;
            SetRowLock(false);

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
            _visitedCells.Add(cell);
        }

        private void SetRowLock(bool isLocked)
        {
            for (var i = 0; i < Board.Width; i++) Board.Cells[i,_pos.y].SetIsLocked(isLocked);
        }
    }
}
