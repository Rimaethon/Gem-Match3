using DG.Tweening;
using Rimaethon.Runtime.UI;
using UnityEngine;

namespace _Scripts.UI.Buttons
{
    public class PanelOpenerButton:UIButton
    {
        [SerializeField] private GameObject _panelToOpen;
        
        protected override void DoOnClick()
        {
            base.DoOnClick();
            _panelToOpen.SetActive(true);
        }
     
    }
}