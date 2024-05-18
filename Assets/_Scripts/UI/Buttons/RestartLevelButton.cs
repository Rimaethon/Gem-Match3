using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace _Scripts.UI.Buttons
{
    public class RestartLevelButton:UIButton
    {
        protected override void DoOnClick()
        {
            base.DoOnClick();
            EventManager.Instance.Broadcast(GameEvents.OnLevelRestart);
        }
    }
}