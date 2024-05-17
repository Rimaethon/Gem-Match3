using UnityEngine;

namespace Scripts.BoosterActions
{
    public interface IItemMergeAction 
    {
        public void InitializeMergeAction(Board board, int item1ID, int item2ID, Vector2Int position1,Vector2Int position2);
        public int Item1ID { get; set; }
        public int Item2ID { get; set; }
        public void Execute();
        public bool IsFinished { get; }
    }
}