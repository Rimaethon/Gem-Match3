using UnityEngine;

namespace Scripts.BoosterActions
{
    public class ObstacleItemAction : IItemAction
    {
        private readonly Board _board;
        private readonly IBoardItem _boardItem;
        private readonly float _explodeTime;
        private float _counter;
        public bool IsFinished { get; set; }

        public ObstacleItemAction(Board board, IBoardItem boardItem, float explodeTime, bool isMatch)
        {
            _boardItem = boardItem;
            _board = board;
            _explodeTime = explodeTime;
        }

        public void InitializeAction()
        {
            ObjectPool.Instance.GetItemParticleEffect(_boardItem.ItemID, _boardItem.Transform.position);
        }

        public void Execute()
        {
            Explode();
        }


        private void Explode()
        {
            if (_counter < _explodeTime)
            {
                _counter += Time.deltaTime;
                return;
            }

            IsFinished = true;
        }
    }
}