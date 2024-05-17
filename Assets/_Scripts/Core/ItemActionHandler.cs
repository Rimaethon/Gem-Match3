using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using Scripts.BoosterActions;
using UnityEngine;

namespace _Scripts.Core
{
    public class ItemActionHandler
    {
        private List<IItemAction> _normalItemActionsToExecute = new List<IItemAction>();
        private List<IItemMergeAction> _mergeActionsToExecute = new List<IItemMergeAction>();
        private List<IItemAction> _actionsToExecuteThisFrame=new List<IItemAction>();
        private List<IItemMergeAction> _mergeActionsToExecuteThisFrame=new List<IItemMergeAction>();
        private readonly Board _board;
        public ItemActionHandler (Board board)
        {
            _board = board;
            EventManager.Instance.AddHandler<Vector2Int,int,int>(GameEvents.AddActionToHandle, AddActionToHandle);
            EventManager.Instance.AddHandler<Vector2Int,Vector2Int>(GameEvents.AddMergeActionToHandle, AddMergeActionToHandle);
        }

     
        public void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector2Int,int,int>(GameEvents.AddActionToHandle, AddActionToHandle);
            EventManager.Instance.RemoveHandler<Vector2Int,Vector2Int>(GameEvents.AddMergeActionToHandle, AddMergeActionToHandle);
        }
        private void AddMergeActionToHandle(Vector2Int pos1,Vector2Int pos2)
        {
            int item1 = _board.GetItem(pos1).ItemID;
            int item2 = _board.GetItem(pos2).ItemID;
            IItemMergeAction mergeAction = ObjectPool.Instance.GetItemMergeAction(item1,item2);
            if (mergeAction == null)
            {
                mergeAction = ObjectPool.Instance.GetItemMergeAction(item2,item1);
                if(mergeAction == null) return;
                mergeAction.InitializeMergeAction(_board,item2,item1,pos1,pos2);
                _mergeActionsToExecute.Add(mergeAction);
                return;
            }
            mergeAction.InitializeMergeAction(_board,item1,item2,pos1,pos2);
            _mergeActionsToExecute.Add(mergeAction);
        }

        private void AddActionToHandle(Vector2Int pos,int value1,int value2)
        {
            IItemAction action = ObjectPool.Instance.GetItemActionFromPool(value1); 
            action.ItemID = value1;
            action.InitializeAction(_board,pos,value1,value2);
            _normalItemActionsToExecute.Add(action);
        }
        public bool HandleActions()
        {
           _actionsToExecuteThisFrame.AddRange( _normalItemActionsToExecute);
            foreach (var itemAction in _actionsToExecuteThisFrame)
            {   
                itemAction.Execute();
                if (!itemAction.IsFinished) continue;
                ObjectPool.Instance.ReturnItemActionToPool(itemAction);
                _normalItemActionsToExecute.Remove(itemAction);
            }
            _actionsToExecuteThisFrame.Clear();
            _mergeActionsToExecuteThisFrame.AddRange(_mergeActionsToExecute);
            foreach (var itemMergeAction in _mergeActionsToExecuteThisFrame)
            {
                itemMergeAction.Execute();
                if (itemMergeAction.IsFinished)
                {
                    _mergeActionsToExecute.Remove(itemMergeAction);
                    ObjectPool.Instance.ReturnItemMergeAction(itemMergeAction,itemMergeAction.Item1ID,itemMergeAction.Item2ID);
                }
            }
            _mergeActionsToExecuteThisFrame.Clear();
            return _normalItemActionsToExecute.Count > 0 || _mergeActionsToExecute.Count > 0;
        }
        
        
    }
}