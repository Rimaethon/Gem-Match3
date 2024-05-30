using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Events
{
    [Serializable]
    public class WheelElement
    {
        public int Amount;
        public bool IsExtraSpin;
        [Range(0.1f,1f)]
        public float Chance=0.1f;
        public GameObject PrizeObjectParent;
        public Image PrizeImage;
        public TextMeshProUGUI PrizeAmountText;
    }
}