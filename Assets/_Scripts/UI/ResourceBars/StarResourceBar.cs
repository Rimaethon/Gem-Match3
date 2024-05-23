using Data;
using DG.Tweening;
using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Panels
{
    public class StarResourceBar : UIButton
    {
        [SerializeField] private TMP_Text _starAmountText;
        [SerializeField] private GameObject starPanel;
        [SerializeField] private bool _isClickAble;
        public RectTransform starIconPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.AddHandler(GameEvents.OnStarAmountChanged, UpdateStarAmount);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnStarAmountChanged, UpdateStarAmount);
        }

        private void Start()
        {
            UpdateStarAmount();
        }
        private void UpdateStarAmount()
        {
            _starAmountText.text = SaveManager.Instance.GetStarAmount().ToString();
        }
        protected override void DoOnClick()
        {
            Debug.Log("Clicked");
            if (!_isClickAble)
            {
                return;
            }
            base.DoOnClick();
            starPanel.SetActive(true);
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