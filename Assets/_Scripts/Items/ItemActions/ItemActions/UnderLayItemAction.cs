using Scripts;
using UnityEngine;

namespace _Scripts.Items.ItemActions
{
    public class UnderLayItemAction: IItemAction
    {
        public bool IsFinished => _isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        private bool _isFinished;

        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2)
        {
            ObjectPool.Instance.GetItemParticleEffect(ItemID, LevelGrid.Instance.GetCellCenterWorld(pos));
            _isFinished = false;
        }

        public void Execute()
        {
            _isFinished = true;
        }
    }
}
