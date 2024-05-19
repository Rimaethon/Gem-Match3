using DG.Tweening;
using Rimaethon.Runtime.UI;
using UnityEngine;

public class CanvasChangerButton : UIButton
{
    
    [SerializeField] private Canvas _canvasToClose;
    [SerializeField] private Canvas _canvasToOpen;

    protected override void DoOnClick()
    {
        base.DoOnClick();
        _canvasToClose.enabled = false;
        _canvasToOpen.enabled = true;
    }
}
