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
        public GameObject clickedCounter;
        public GameObject unlimitedCounter;
        public TextMeshProUGUI unlimitedCounterText;
        public bool isClicked;
        public RectTransform boosterTransform;
        public GameObject boosterPanel;
    }
}