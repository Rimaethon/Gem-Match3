using System.Collections;
using System.Collections.Generic;
using Rimaethon.Runtime.UI;
using UnityEngine;

public class PanelCloserButton : UIButton
{
    [SerializeField] private GameObject panel;

    protected override void DoOnClick()
    {
        base.DoOnClick();
        panel.SetActive(false);
    }
}
