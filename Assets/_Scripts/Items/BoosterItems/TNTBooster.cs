using System;
using System.Collections.Generic;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts
{
    public class TNTBooster:ItemBase
    {
        private TNTBoosterAction _tntBoosterAction = new TNTBoosterAction();

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
            IsMatching = false;

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
            _tntBoosterAction.Execute(Board,Position,_itemID);
            OnRemove();
        }
        public override void OnRemove()
        {
            ObjectPool.Instance.ReturnItem(Item, ItemID);
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
        }
     
    }
}