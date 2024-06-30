using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class MissileBooster:BoosterBoardItem
    {
        public override void OnSwap(IBoardItem boardItem, IBoardItem otherBoardItem)
        {
            if(IsExploding||!IsActive)
                return;
            OnExplode();
        }

        public override void OnClick(Board board, Vector2Int pos)
        {
            if(IsMoving||IsExploding||IsSwapping||_isClicked||IsMatching)
                return;
            _isClicked = true;
            OnExplode();
        }

        public override void OnExplode()
        {
            if(IsExploding||!IsActive)
                return;
            _isExploding= true;
            OnRemove();
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, Position, ItemID, -1);

        }

        public override void OnRemove()
        {
            EventManager.Instance.Broadcast<Vector2Int>(GameEvents.AddItemToRemoveFromBoard, Position);
        }
    }


}
