using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

namespace _Scripts.Items.ItemActions.MergeActions
{
    public class MissileVerticalRocketMergeAction : IItemMergeAction
    {
        public bool IsFinished => _isFinished;
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }
        private bool _isFinished;
        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1, Vector2Int position2)
        {
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,position2,item1ID,item2ID);

            _isFinished = false;
        }

        public void Execute()
        {
            _isFinished = true;
        }
    }
}
