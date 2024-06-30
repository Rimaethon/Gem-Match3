using UnityEngine;

namespace Scripts
{
    public interface  IItemAction
    {
        //Im using this because I want some way to control when to create GameObjects other than constructor of the class 
        public void InitializeAction(Board board, Vector2Int pos, int value1, int value2);
        public void Execute();
        public bool IsFinished { get; }
        public int ItemID { get; set; }
        public Board Board { set; }
      
    }
}
