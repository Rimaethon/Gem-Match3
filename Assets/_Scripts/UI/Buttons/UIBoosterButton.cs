using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    [Serializable]
    public class UIBoosterButton
    {
        public int itemID;
        public TextMeshProUGUI boosterCounter;
        public Button boosterButton;
        public Image boosterBackground;
        public GameObject unClickedCounter;
        //This will be used as background particle effect in In Game Power Up Panel
        public GameObject clickedCounter;
        public GameObject unlimitedCounter;
        public bool isClicked;
        public RectTransform boosterTransform;
        public GameObject boosterPanel;
    }
}