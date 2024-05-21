using System;
using Data;
using DG.Tweening;
using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.UI.Panels
{
    public class CoinResourceBar:UIButton
    {
        [SerializeField] private TMP_Text _coinText;
        [SerializeField] private GameObject coinPanel;
        [SerializeField] private bool _isClickAble;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.AddHandler(GameEvents.OnCoinAmountChanged, UpdateCoinAmount);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnCoinAmountChanged, UpdateCoinAmount);
        }
        private void Start()
        {
            UpdateCoinAmount();
        }

        private void UpdateCoinAmount()
        {
            _coinText.text=SaveManager.Instance.GetCoinAmount().ToString();

        }
        protected override void DoOnClick()
        {
            Debug.Log("Clicked");
            if (!_isClickAble&&coinPanel!=null)
            {
                return;
            }
            base.DoOnClick();
            coinPanel.SetActive(true);
        }

        protected override void DoOnPointerDown()
        {
            if (!_isClickAble)
            {
                return;
            }

            base.DoOnPointerDown();
        }

        protected override void DoOnPointerUp()
        {
            if (!_isClickAble)
            {
                return;
            }
            base.DoOnPointerUp();
        }
    }
}