using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts.BoosterActions;
using UnityEngine;
namespace Scripts
{
    public class ObstacleBoardItem:BoardItemBase
    {
        private ObstacleItemAction _action;
        private void OnEnable()
        {
            transform.localScale = Vector3.one;
            isFallAble = false;
            _isMatchable = false;
            _isSwappable = false;
            _isExplodeAbleByNearMatches = true;
            _isExploding = false;
            //I couldn't find any reason behind this but somehow this is getting set to true at start but it should be false
            _isGeneratorItem = false;
            _isProtectingUnderIt = true;
            Highlight(0);
        }
        public override void OnExplode()
        {
            if(_isExploding)
                return;
            _isExploding = true;
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,0);
            OnRemove();
        }
        public override void OnRemove()
        {
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
            EventManager.Instance.Broadcast(GameEvents.OnItemExplosion, _position, _itemID);
        }
        public override void OnClick(Board board, Vector2Int pos)
        {
            if (IsMoving || _isExploding||_isClicked|| IsMatching)
                return;
            _isClicked = true;
            transform.DOPunchRotation(new Vector3(0, 0, 20), 0.2f).SetUpdate(UpdateType.Fixed).
                OnComplete(() =>
                {
                    _isClicked = false;
                });            
        }
    }
}