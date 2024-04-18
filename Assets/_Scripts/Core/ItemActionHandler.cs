using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Core
{
    public class ItemActionHandler
    {
        private readonly HashSet<IItemAction> _actionsToExecute = new HashSet<IItemAction>();
        private readonly HashSet<IItemAction> _actionsToExecuteThisFrame = new HashSet<IItemAction>();
        private Board _board;
        public ItemActionHandler (Board board)
        {
            _board = board;
            EventManager.Instance.AddHandler<IItemAction>(GameEvents.AddActionToHandle, AddActionToHandle);
        }
       
        
        private void AddActionToHandle(IItemAction itemPos)
        {
            _actionsToExecute.Add(itemPos);
            
        }

        public void HandleActions()
        {
            _actionsToExecuteThisFrame.UnionWith(_actionsToExecute);
            
            foreach (IItemAction itemPos in _actionsToExecuteThisFrame)
            {
               itemPos.Execute();
               if(itemPos.IsFinished)
                   _actionsToExecute.Remove(itemPos);
            }
            _actionsToExecuteThisFrame.Clear();

        }
        
    }
}