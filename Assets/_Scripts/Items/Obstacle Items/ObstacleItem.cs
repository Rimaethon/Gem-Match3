using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class ObstacleItem:ItemBase
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
            ObjectPool.Instance.GetItemParticleEffect(_itemID, Transform.position);
            ObjectPool.Instance.ReturnItem(Item, _itemID);
            EventManager.Instance.Broadcast<Vector2Int>(GameEvents.AddItemToRemoveFromBoard, _position);

        }
    
        public override void OnClick(Board board, Vector2Int pos)
        {
            if (IsMoving || IsExploding||_isClicked)
                return;
            Debug.Log("MatchItem Clicked"+pos);
            transform.DOPunchRotation(new Vector3(0, 0, 20), 0.5f).OnComplete(() => {_isClicked = false;});            
        }
        

    }
}