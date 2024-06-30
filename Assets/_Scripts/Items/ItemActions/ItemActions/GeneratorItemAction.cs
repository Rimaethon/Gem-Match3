using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions
{
    public class GeneratorItemAction : IItemAction
    {
        private Vector2Int _pos;
        private Vector3 _goalUIPos;
        private readonly Vector3 _moveUpOffset = new(0, 0.4f, 0);
        private GameObject _particleEffect;
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private float moveUpAndScaleTime = 0.4f;
        private float moveUpAndScaleCounter;
        private float waitTime = 0.1f;
        private float waitCounter;
        private float moveUpTime = 0.5f;
        private float moveUpCounter;
        private Vector3 initalPos;
        private Vector3 moveUpPos;
        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            _pos = pos;
            Board = board;
            IsFinished = false;
            _particleEffect = null;
            _goalUIPos = InGameUIManager.Instance.GetGoalPosition(value1);
            initalPos = LevelGrid.Instance.GetCellCenterWorld(pos);
            moveUpPos = initalPos + _moveUpOffset;
            _particleEffect = ObjectPool.Instance.GetItemParticleEffect(value1,initalPos);
            ItemID = value1;
            moveUpCounter = 0;
            moveUpAndScaleCounter = 0;
            waitCounter = 0;
        }
        public void Execute()
        {
            if (moveUpAndScaleCounter < moveUpAndScaleTime)
            {
                moveUpAndScaleCounter += Time.fixedDeltaTime;
                _particleEffect.transform.position = Vector3.Lerp(_particleEffect.transform.position, moveUpPos, moveUpAndScaleCounter / moveUpAndScaleTime);
                _particleEffect.transform.localScale = Vector3.Lerp(_particleEffect.transform.localScale, Vector3.one, moveUpAndScaleCounter / moveUpAndScaleTime);
                return;
            }
            if(waitCounter<waitTime)
            {
                initalPos=_particleEffect.transform.position;
                waitCounter += Time.fixedDeltaTime;
                return;
            }
            if(moveUpCounter<moveUpTime)
            {
                moveUpCounter += Time.fixedDeltaTime;
                _particleEffect.transform.position = Vector3.Lerp(initalPos, _goalUIPos, moveUpCounter / moveUpTime);
                return;
            }
            EventManager.Instance.Broadcast(GameEvents.OnItemExplosion, _pos, ItemID);
            IsFinished = true;
            ObjectPool.Instance.ReturnParticleEffect(_particleEffect,ItemID);
        }
    }
}
