using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace _Scripts.Core
{
    public class ItemActionHandler
    {
        private List<IItemAction> _actionsToExecute = new List<IItemAction>();
        private List<IItemAction> _actionsToExecuteThisFrame;
        private Board _board;
        public ItemActionHandler (Board board)
        {
            _board = board;
            EventManager.Instance.AddHandler<IItemAction>(GameEvents.AddActionToHandle, AddActionToHandle);
        }
       
        
        private void AddActionToHandle(IItemAction itemPos)
        {
            itemPos.InitializeAction();
            _actionsToExecute.Add(itemPos);
            itemPos = null;
        }

        public void HandleActions()
        {
           _actionsToExecuteThisFrame = new List<IItemAction>(_actionsToExecute);
            for (int i=0;i<_actionsToExecuteThisFrame.Count ;i++)
            {
                _actionsToExecuteThisFrame[i].Execute();
                if (_actionsToExecuteThisFrame[i].IsFinished)
                {
                    _actionsToExecute.Remove(_actionsToExecuteThisFrame[i]);
                    _actionsToExecuteThisFrame[i]=null;
                }
            }
            _actionsToExecuteThisFrame.Clear();
       
            
            
        }
        
        
    }
}