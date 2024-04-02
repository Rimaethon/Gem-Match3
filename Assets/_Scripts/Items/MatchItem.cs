using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts
{
    public class MatchItem:ItemBase
    {
        private MatchItemAction _action;
        [SerializeField] float _scaleDownTime;
        private void OnEnable()
        {
            transform.localScale = Vector3.one;
            _isBooster = false;
            _isSwappable = true;
            _isMatchable = true;
            isFallAble = true;
            IsActive = true;
            _isExploding = false;
            _isHighlightAble = true;
            IsMoving = false;
            IsMatching = false;
            Highlight(0);
            
        }
     
        public override void OnExplode()
        {
            if(_isExploding)
                return;
            _isExploding = true;
            _action= new MatchItemAction(Board,Item,_scaleDownTime,false);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, _action);
        }

        public override void OnMatch()
        {
            if(_isExploding||!IsMatching)
                return;
            _isExploding = true;
            _action= new MatchItemAction(Board,Item,_scaleDownTime,true);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, _action);
        }

        public override void OnRemove()
        {
            ObjectPool.Instance.ReturnItem(Item, ItemID);
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
        }
        public override void OnClick(Board board, Vector2Int pos)
        {
            if (IsMoving || _isExploding||_isClicked|| IsMatching)
                return;
            _isClicked = true;
            Debug.Log("MatchItem Clicked"+pos);
            transform.DOPunchRotation(new Vector3(0, 0, 20), 0.2f).OnComplete(() => {_isClicked = false;});            
            
        }
    }
}