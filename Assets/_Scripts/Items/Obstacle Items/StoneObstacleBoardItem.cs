using Rimaethon.Scripts.Managers;

namespace Scripts
{
    public class StoneObstacleBoardItem:ObstacleBoardItem
    {
        public override void OnExplode()
        {
            if(_isExploding)
                return;
            _isExploding = true;
            AudioManager.Instance.PlaySFX(SFXClips.StoneBreakingSound);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,0);
        }
    }
}
