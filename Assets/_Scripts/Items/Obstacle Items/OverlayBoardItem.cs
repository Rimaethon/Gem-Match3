using Rimaethon.Scripts.Managers;

namespace Scripts
{
    public class OverlayBoardItem:BoardItemBase
    {
        protected override void Awake()
        {
            base.Awake();
            isFallAble = false;
            _isMatchable = false;
            _isSwappable = false;
            _isProtectingUnderIt = true;
            _isExplodeAbleByNearMatches = true;
        }
        public override void OnExplode()
        {
            if(IsExploding)
                return;
            _isExploding= true;
            ObjectPool.Instance.GetItemParticleEffect(_itemID, Transform.position);
            OnRemove();
        }

        public override void OnRemove()
        {
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
            EventManager.Instance.Broadcast(GameEvents.OnItemRemoval, BoardItem);
            Board.Cells[Position.x, Position.y].SetOverLayItem(null);
        }
    }
}
