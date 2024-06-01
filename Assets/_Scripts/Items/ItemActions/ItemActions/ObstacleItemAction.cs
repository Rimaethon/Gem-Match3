using UnityEngine;

namespace Scripts.BoosterActions
{
    public class ObstacleItemAction : IItemAction
    {
        private readonly float _explodeTime=0.2f;
        private float _counter;
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            Board = board;
            ObjectPool.Instance.GetItemParticleEffect(ItemID, LevelGrid.Instance.GetCellCenterWorld(pos));
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