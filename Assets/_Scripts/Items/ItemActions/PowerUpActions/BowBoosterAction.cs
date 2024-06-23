using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Items.ItemActions
{
    public class BowBoosterAction:IItemAction
    {
        public bool IsFinished=>_isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private bool _isFinished;
        private Vector2Int _pos;
        private Vector3 _startWorldPos;
        private Vector3 _endWorldPos;
        private BowParticleEffect particleEffect;
        private float _speed = 8f;
        private readonly HashSet<Vector2Int> _visitedCells = new();
        private readonly float _beforeFiringWait = 0.1f;
        private float _beforeFiringTimer;
        private readonly float _afterFiringWait = 0.3f;
        private float _afterFiringTimer;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            Board = board;
            _pos = new Vector2Int(0,pos.y);
            _startWorldPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            _startWorldPos.x -= 0.7f;
            _endWorldPos = LevelGrid.Instance.GetCellCenterWorld(new Vector2Int(Board.Width-1,pos.y));
            _endWorldPos.x += 0.7f;
            particleEffect= ObjectPool.Instance.GetBoosterParticleEffect(ItemID,_startWorldPos).GetComponent<BowParticleEffect>();
            _isFinished = false;
            SetRowLock(true);
            _beforeFiringTimer = 0;
            _afterFiringTimer = 0;
            _visitedCells.Clear();
        }

        public void Execute()
        {
            if(_beforeFiringTimer < _beforeFiringWait)
            {
                _beforeFiringTimer += Time.fixedDeltaTime;
                return;
            }
            if (particleEffect.hitPoint.transform.position.x < _endWorldPos.x)
            {
                particleEffect.gameObject.transform.position += Vector3.right * (_speed * Time.fixedDeltaTime);
        
                var arrowCell = LevelGrid.Instance.WorldToCellVector2Int(particleEffect.hitPoint.transform.position);
                if (Board.IsInBoundaries(arrowCell) && Board.Cells[arrowCell.x,arrowCell.y].HasItem &&!Board.GetItem(arrowCell).IsExploding)
                {
                    if (!_visitedCells.Contains(arrowCell) ||!Board.GetItem(arrowCell).IsGeneratorItem)
                    {
                        Board.GetItem(arrowCell).OnExplode();
                        _visitedCells.Add(arrowCell);
                    }
                }
                return;
            }
            if(_afterFiringTimer < _afterFiringWait)
            {
                _afterFiringTimer += Time.fixedDeltaTime;
                return;
            }
      
            ObjectPool.Instance.ReturnBoosterParticleEffect(particleEffect.gameObject, ItemID);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
            _isFinished = true;
            SetRowLock(false);

        }

        private void SetRowLock(bool isLocked)
        {
            for (var i = 0; i < Board.Width; i++) Board.Cells[i,_pos.y].SetIsLocked(isLocked);
        }

    }
}
