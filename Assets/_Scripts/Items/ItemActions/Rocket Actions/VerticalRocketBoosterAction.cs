﻿using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class VerticalRocketBoosterAction : IItemAction
    {
        private float _boardDownEdge;
        private float _boardUpEdge;
        private readonly float _rocketOffset = 0.1f;
        private readonly float _speed = 6f;
        private readonly HashSet<Vector2Int> _visitedCells = new();
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
            upRocketPos.y -= _rocketOffset;
            downRocketPos.y += _rocketOffset;
            _upRocket = ObjectPool.Instance.GetBoosterParticleEffect(ItemID, upRocketPos, Quaternion.Euler(0, 0, -90))
                .transform;
            _downRocket = ObjectPool.Instance
                .GetBoosterParticleEffect(ItemID, downRocketPos, Quaternion.Euler(0, 0, 90)).transform;
            Board.Cells[_pos.x,_pos.y].SetIsLocked(true);
            if (Board.Cells[_pos.x,_pos.y].HasItem && !Board.GetItem(_pos).IsExploding)
                Board.GetItem(_pos).OnExplode();
            IsFinished = false;
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
                    if (Board.IsInBoundaries(upRocketCell) && Board.Cells[upRocketCell.x,upRocketCell.y].HasItem &&
                        !Board.GetItem(upRocketCell).IsExploding)
                    {
                        if (!_visitedCells.Contains(upRocketCell) ||!Board.GetItem(upRocketCell).IsGeneratorItem)
                        {
                            Board.GetItem(upRocketCell).OnExplode();
                        }
                        if (!_visitedCells.Contains(upRocketCell))
                        {
                            Board.Cells[upRocketCell.x,upRocketCell.y].SetIsLocked(true);
                            _visitedCells.Add(upRocketCell);
                        }
                    }

                    if (Board.IsInBoundaries(downRocketCell) && Board.Cells[downRocketCell.x,downRocketCell.y].HasItem &&
                        !Board.GetItem(downRocketCell).IsExploding)
                    {
                        if (!_visitedCells.Contains(downRocketCell) ||!Board.GetItem(downRocketCell).IsGeneratorItem)
                        {
                            Board.GetItem(downRocketCell).OnExplode();
                        }
                        if (!_visitedCells.Contains(downRocketCell))
                        {
                            Board.Cells[downRocketCell.x,downRocketCell.y].SetIsLocked(true);
                            _visitedCells.Add(downRocketCell);
                        }
                    }
                }
                
                return;
            }
            ObjectPool.Instance.ReturnBoosterParticleEffect(_upRocket.gameObject, ItemID);
            ObjectPool.Instance.ReturnBoosterParticleEffect(_downRocket.gameObject, ItemID);
            IsFinished = true;
            SetColumnLock(false);

        }

        private void SetColumnLock(bool isLocked)
        {
            for (var i = 0; i < Board.Height; i++) Board.Cells[_pos.x,i].SetIsLocked(isLocked);
        }
    }
}