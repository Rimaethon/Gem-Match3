using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class GeneratorItemAction : IItemAction
    {
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private GameObject _particleEffect;
        private Vector2Int _pos;
        private Vector3 _goalUIPos;
        private readonly Vector3 _moveUpOffset = new(0, 0.4f, 0);
        private float _moveUpAndScaleTime = 0.4f;
        private float _moveUpAndScaleCounter;
        private float _waitTime = 0.1f;
        private float _waitCounter;
        private float _moveUpTime = 0.5f;
        private float _moveUpCounter;
        private Vector3 _initialPos;
        private Vector3 _moveUpPos;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            _pos = pos;
            Board = board;
            IsFinished = false;
            _particleEffect = null;
            _goalUIPos = InGameUIManager.Instance.GetGoalPosition(value1);
            _initialPos = LevelGrid.Instance.GetCellCenterWorld(pos);
            _moveUpPos = _initialPos + _moveUpOffset;
            _particleEffect = ObjectPool.Instance.GetItemParticleEffect(value1,_initialPos);
            ItemID = value1;
            _moveUpCounter = 0;
            _moveUpAndScaleCounter = 0;
            _waitCounter = 0;
        }

        public void Execute()
        {
            if (_moveUpAndScaleCounter < _moveUpAndScaleTime)
            {
                _moveUpAndScaleCounter += Time.fixedDeltaTime;
                _particleEffect.transform.position = Vector3.Lerp(_particleEffect.transform.position, _moveUpPos, _moveUpAndScaleCounter / _moveUpAndScaleTime);
                _particleEffect.transform.localScale = Vector3.Lerp(_particleEffect.transform.localScale, Vector3.one, _moveUpAndScaleCounter / _moveUpAndScaleTime);
                return;
            }
            if(_waitCounter<_waitTime)
            {
                _initialPos=_particleEffect.transform.position;
                _waitCounter += Time.fixedDeltaTime;
                return;
            }
            if(_moveUpCounter<_moveUpTime)
            {
                _moveUpCounter += Time.fixedDeltaTime;
                _particleEffect.transform.position = Vector3.Lerp(_initialPos, _goalUIPos, _moveUpCounter / _moveUpTime);
                return;
            }
            EventManager.Instance.Broadcast(GameEvents.OnItemExplosion, _pos, ItemID);
            IsFinished = true;
            ObjectPool.Instance.ReturnParticleEffect(_particleEffect,ItemID);
        }
    }
}
