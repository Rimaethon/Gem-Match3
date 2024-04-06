using UnityEngine;

namespace Scripts
{
    public interface IItemAction
    {
        public void Execute();
        public bool IsFinished { get; }
    }
}
// Path: Assets/_Scripts/Items/ItemActions/TNTBoosterAction.cs                                  