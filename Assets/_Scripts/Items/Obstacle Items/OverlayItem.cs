using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class OverlayItem:ItemBase
    {
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
            ObjectPool.Instance.GetItemParticleEffect(_itemID, Transform.position);
            await UniTask.Delay(200);
        }
       
    

    }
}