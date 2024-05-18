using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;

namespace _Scripts.UI.Buttons
{
    public class ReturnToMainMenuButton:UIButton
    {
        protected override void DoOnClick()
        {
            base.DoOnClick();
            EventManager.Instance.Broadcast(GameEvents.OnLevelFailed);
        }
    }
}