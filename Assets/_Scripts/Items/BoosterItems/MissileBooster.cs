using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Managers;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts
{
    public class MissileBooster:ItemBase
    {
        private MissileBoosterAction _action;

        private void OnEnable()
        {
            transform.localScale = Vector3.one;
            _isBooster = true;
            _isSwappable = true;
            _isMatchable = false;
            isFallAble = true;
            IsActive = true;
            _isExploding = false;
            _isHighlightAble = true;
            IsMoving = false;
        }

    
        public override void OnClick(Board board, Vector2Int pos)
        {
            if(IsMoving||IsExploding)
                return;
            OnExplode();
        }
        public override void OnExplode()
        {
            if(IsExploding)
                return;
            _isExploding= true;
            Vector2Int goalPos = LevelManager.Instance.GetRandomGoalPos();
            if (goalPos is { x: -1, y: -1 })
            {
                return;
            }
            _action = new MissileBoosterAction(goalPos);
            _action.Execute(Board, Position, _itemID);
            OnRemove();
        }
        public override void OnRemove()
        {
            ObjectPool.Instance.ReturnItem(Item, ItemID);
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
        }
    }
}