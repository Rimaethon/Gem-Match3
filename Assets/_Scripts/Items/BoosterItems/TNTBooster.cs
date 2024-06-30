using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class TNTBooster:BoosterBoardItem
    {

        public override void OnSwap(IBoardItem boardItem, IBoardItem otherBoardItem)
        {
            if(IsMoving||IsExploding||_isClicked)
                return;
            _isClicked = true;
            OnExplode();
        }

        public override void OnClick(Board board, Vector2Int pos)
        {
            if(IsMoving||IsExploding||_isClicked)
                return;
            _isClicked = true;
            OnExplode();
        }

        public override void OnExplode()
        {
            if(IsExploding||!IsActive)
                return;
            _isExploding = true;
            AudioManager.Instance.PlaySFX(SFXClips.TNTSound);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, Position, _itemID, -1);
            OnRemove();
        }

        public override void OnRemove()
        {
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
        }

    }
}
