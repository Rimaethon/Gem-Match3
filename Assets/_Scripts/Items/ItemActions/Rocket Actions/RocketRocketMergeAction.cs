using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions.MergeActions
{
    public class RocketRocketMergeAction : IItemMergeAction
    {
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }

        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1, Vector2Int position2)
        {
            Board = board;
            Item1ID = item1ID;
            Item2ID = item2ID;
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, position2, 101, 0);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, position2, 102, 0);
            IsFinished = false;
        }

        public void Execute()
        {
            IsFinished = true;
        }
    }
}
