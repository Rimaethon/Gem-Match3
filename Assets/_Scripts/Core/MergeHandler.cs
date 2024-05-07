using System;
using System.Collections.Generic;
using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

namespace _Scripts.Managers.Matching
{
    public class MergeHandler
    {
        private List<Tuple<int, int, IItemMergeAction>> _boosterMergeAction;
        private Board _board;
        public MergeHandler(Board board)
        {
            _board = board;
            _boosterMergeAction =ObjectPool.Instance.itemDatabase.BoosterMergeAction;
        }
        public void MergeItems(IBoardItem item1,IBoardItem item2)
        {
            foreach (var merge in _boosterMergeAction)
            {
                if (merge.Item1 == item1.ItemID && merge.Item2 == item2.ItemID||merge.Item1 == item2.ItemID && merge.Item2 == item1.ItemID)
                {
                    Debug.Log("Merge Found");
                    item1.OnRemove();
                    item2.OnRemove();
                   IItemMergeAction action= merge.Item3;
                   Debug.Log("Merge Action Initialized"+merge.Item1+" "+merge.Item2);
                   action.InitializeMergeAction(item2.Board,merge.Item1,merge.Item2, item2.Position);
                   EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, action);
                }
            }
        }
        
        
    }
}