using System.Collections.Generic;
using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts.BoosterActions;
using UnityEngine;

namespace Scripts
{
    // https://youtu.be/iSaTx0T9GFw?t=947
    //As can be seen in the bottom left of the board in the video, horizontal rocket is not always locking the cell and sometimes can make mistakes such as not exploding the item. 
    //But this doesnt mean anything to me since It can be an intended or unintended thing
    //What i will do is I will lock the cell for future movements and when rocket comes to that cell it will check if there is any item to explode.
    //This will make items which on halfway explode without directly contacting with the rocket but this is not a precision that I'm looking for in this project. 
    public class HorizontalRocketBooster:BoosterBoardItem
    {
        private HorizontalRocketBoosterAction _action;
        
        public override void OnClick(Board board, Vector2Int pos)
        {
            if(IsMoving||IsExploding||IsSwapping||_isClicked)
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
            AudioManager.Instance.PlaySFX(SFXClips.Rocket);
            EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,  Position,_itemID,-1);
        }
        public override void OnSwap(IBoardItem boardItem, IBoardItem otherBoardItem)
        {
            OnExplode();
        }
        public override void OnRemove()
        {
            EventManager.Instance.Broadcast(GameEvents.AddItemToRemoveFromBoard, Position);
        }


    }
}