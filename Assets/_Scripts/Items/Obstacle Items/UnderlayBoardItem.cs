using Rimaethon.Scripts.Managers;

namespace Scripts
{
    public class UnderlayBoardItem:BoardItemBase
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
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,0);
            OnRemove();
        }

        public override void OnRemove()
        {
            EventManager.Instance.Broadcast(GameEvents.OnItemExplosion, _position, _itemID);

            ObjectPool.Instance.ReturnItem(BoardItem, _itemID);
            Board.Cells[Position.x, Position.y].SetUnderLayItem(null);

        }
    }
}
