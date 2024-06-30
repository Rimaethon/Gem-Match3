using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class MatchItem:BoardItemBase
    {
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
            _isShuffleAble = true;
            Highlight(0);
        }

        public override void OnExplode()
        {
            if(_isExploding||IsMatching||!_isActive)
                return;
            _isExploding = true;
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, Position, _itemID, -1);
            EventManager.Instance.Broadcast(GameEvents.OnItemExplosion, _position, _itemID);

        }

        public override void OnMatch()
        {
            if(_isExploding||!IsMatching)
                return;
            _isExploding = true;
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,ItemID,0);
        }

        public override void OnRemove()
        {
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
            EventManager.Instance.Broadcast(GameEvents.OnItemRemoval, BoardItem);
        }

        public override void OnClick(Board board, Vector2Int pos)
        {
        }
    }
}
