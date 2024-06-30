using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Items.ItemActions
{
    public class ExplodeAction
    {
        public bool _isFinished;
        protected Vector2Int _pos;
        protected Board _board;
        private const float ExplosionDelay = 0.075f;
        private int _radius=2;
        private float _counter;
        private int _currentExplosionArea = 1;

        protected void InitializeExplode(Board board,int radius, Vector2Int pos)
        {
            _board = board;
            _radius = radius;
            _pos = pos;
            _isFinished = false;
            _counter = 0.0f;
            _currentExplosionArea = 1;
            EventManager.Instance.Broadcast(GameEvents.OnBoardShake);
            SetIsLocked(_pos, _radius,true);
            if (_board.Cells[_pos.x,_pos.y].HasItem && !_board.GetItem(_pos).IsExploding)
                _board.GetItem(_pos).OnExplode();
        }

        protected void HandleExplosion()
        {
            if (_currentExplosionArea <= _radius)
            {
                if (_counter < ExplosionDelay)
                {
                    _counter += Time.deltaTime;
                    return;
                }

                ExpandAndDestroy();
                _currentExplosionArea++;
                _counter = 0.0f;
            }
            else
            {
                SetIsLocked( _pos, _radius,false);
                _isFinished = true;
            }
        }

        private void ExpandAndDestroy()
        {
            // Explode cells at the current explosion radius
            for (int x = _pos.x - _currentExplosionArea; x <= _pos.x + _currentExplosionArea; x++)
            {
                ExplodeCell(new Vector2Int(x, _pos.y - _currentExplosionArea));
                ExplodeCell(new Vector2Int(x, _pos.y + _currentExplosionArea));
            }
            for (int y = _pos.y - _currentExplosionArea + 1; y < _pos.y + _currentExplosionArea; y++)
            {
                ExplodeCell(new Vector2Int(_pos.x - _currentExplosionArea, y));
                ExplodeCell(new Vector2Int(_pos.x + _currentExplosionArea, y));
            }
        }

        private void ExplodeCell(Vector2Int explosionPos)
        {
            if (!_board.IsInBoundaries(explosionPos)) return;
            if (!_board.Cells[explosionPos.x, explosionPos.y].HasItem)
            {
                EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, explosionPos);
                return;
            }
            if (!_board.GetItem(explosionPos).IsExploding)
            {
                _board.GetItem(explosionPos).OnExplode();
            }
        }

        private void SetIsLocked(Vector2Int pos, int lockArea,bool isLocked)
        {
            for (var x = pos.x - lockArea; x <= pos.x + lockArea; x++)
            {
                for (var y = pos.y - lockArea; y <= pos.y + lockArea; y++)
                {
                    if (_board.IsInBoundaries(new Vector2Int(x, y)))
                    {
                        _board.Cells[x,y].SetIsLocked(isLocked);
                    }
                }
            }
        }
    }
}
