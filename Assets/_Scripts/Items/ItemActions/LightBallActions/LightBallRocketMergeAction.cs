using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts.BoosterActions.MergeActions
{
    public class LightBallRocketMergeAction : IItemMergeAction
    {
        public bool IsFinished { get; set; }
        public int ItemID { get; set; }
        public Board Board { get; set; }
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }

        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1, Vector2Int position2)
        {
            Item1ID = item1ID;
            Item2ID = item2ID;
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,position2,item1ID,item2ID);

        }
        public void Execute()
        {
            IsFinished = true;
        }

    }
}