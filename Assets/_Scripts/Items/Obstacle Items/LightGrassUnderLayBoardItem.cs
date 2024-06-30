using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    //Should this class inherit from ItemBase or not ? I'm asking this question for every item class I create. Maybe all of us are classes that inherit from ItemBase in a parallel universe.
    public class LightGrassUnderLayBoardItem:UnderlayBoardItem
    {

        public override void OnExplode()
        {
            if(IsExploding)
                return;
            _isExploding= true;
            int random = Random.Range(0, 1);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,0);
            AudioManager.Instance.PlaySFX(random == 1 ? SFXClips.BushSound : SFXClips.GrassSound);
            OnRemove();
        }
    }
}
