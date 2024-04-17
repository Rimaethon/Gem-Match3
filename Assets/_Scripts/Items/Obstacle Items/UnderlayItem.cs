using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    //Items such as grass that has no effect on other items but can be a goal for the player
    public class UnderlayItem:ItemBase
    {
        protected GameObject _itemParticleEffect;
        protected override void Awake()
        {
            base.Awake();
            isFallAble = false;
            _isMatchable = false;
            _isSwappable = false;
        }
        public override void OnExplode()
        {
            if(IsExploding)
                return;
            _isExploding= true;
            ExplodeObject().Forget();
        }
        private async UniTask ExplodeObject()
        {
            ObjectPool.Instance.ReturnItem(Item, _itemID);
            _itemParticleEffect=ObjectPool.Instance.GetItemParticleEffect(_itemID, Transform.position);
            await UniTask.Delay(100);
        }
     
    }
}