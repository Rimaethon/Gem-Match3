using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Items.ItemActions
{
    public class CannonBoosterAction:IItemAction
    {
        public bool IsFinished=>_isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private bool _isFinished;
        private Vector2Int _pos;
        private Vector3 _startWorldPos;
        private Vector3 _endWorldPos;
        private CannonParticleEffect particleEffect;
        private float _speed = 10f;
        private readonly HashSet<Vector2Int> _visitedCells=new HashSet<Vector2Int>();
        private Vector3 _initialSpriteLocalPosition=new Vector3(0,0.1f,0);
        private readonly float _beforeFiringWait = 0.1f;
        private float _beforeFiringTimer;
        private readonly float _afterFiringWait = 0.5f;
        private float _afterFiringTimer;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            Board = board;
            _pos = new Vector2Int(pos.x,0);
            _startWorldPos = LevelGrid.Instance.GetCellCenterWorld(_pos);
            _startWorldPos.y -= 0.85f;
            _endWorldPos = LevelGrid.Instance.GetCellCenterWorld(new Vector2Int(pos.x,Board.Height-1));
            _endWorldPos.y += 2f;
            particleEffect= ObjectPool.Instance.GetBoosterParticleEffect(ItemID,_startWorldPos).GetComponent<CannonParticleEffect>();
            _isFinished = false;
            _visitedCells.Clear();

        }

        public void Execute()
        {
            if(_beforeFiringTimer < _beforeFiringWait)
            {
                _beforeFiringTimer += Time.fixedDeltaTime;
                return;
            }
            if (particleEffect.hitPoint.transform.position.y < _endWorldPos.y)
            {
                particleEffect.cannonBallSprite.transform.position += Vector3.up * (_speed * Time.fixedDeltaTime);

                var ballCell = LevelGrid.Instance.WorldToCellVector2Int(particleEffect.hitPoint.transform.position);
                HandleExplosion(ballCell);
                return;
            }
            if(_afterFiringTimer < _afterFiringWait)
            {
                _afterFiringTimer += Time.fixedDeltaTime;
                return;
            }
            particleEffect.cannonBallSprite.transform.localPosition = _initialSpriteLocalPosition;
            ObjectPool.Instance.ReturnBoosterParticleEffect(particleEffect.gameObject, ItemID);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
            _isFinished = true;
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
