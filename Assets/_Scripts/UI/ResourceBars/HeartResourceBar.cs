using System;
using _Scripts.Core.Interfaces;
using Data;
using DG.Tweening;
using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Panels
{
    public class HeartResourceBar:UIButton,ITimeDependent
    {
        [SerializeField] private TMP_Text _heartText;
        [SerializeField] TMP_Text _heartRefillTimeText;
        [SerializeField] private GameObject heartPanel;
        private bool _isClickAble;


        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.AddHandler(GameEvents.OnHeartAmountChanged, UpdateHeartVisual);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnHeartAmountChanged, UpdateHeartVisual);
        }

        
        public void OnTimeUpdate(long currentTime)
        {
            if (SaveManager.Instance.HasUnlimitedHearts())
            {
                _heartRefillTimeText.text = SaveManager.Instance.GetUnlimitedHeartTime();
                return;
            }
            UpdateHeartVisual();
        }
        private void Start()
        {
            UpdateHeartVisual();  
        }

        private void UpdateHeartVisual()
        {
            if(SaveManager.Instance.HasUnlimitedHearts())
            {
                _heartText.text="∞";
            }
            else
            {
                int heartCount=SaveManager.Instance.GetHeartAmount();
                if (heartCount == SaveManager.Instance.GetMaxHeartAmount())
                {
                    _heartRefillTimeText.text = "Full";
                }
                else
                {
                    heartCount=(int)Math.Clamp((DateTimeOffset.UtcNow.ToUnixTimeSeconds()-SaveManager.Instance.GetFirstHeartNotBeingFullUnixTime())/SaveManager.Instance.GetHeartRefillTimeInSeconds(),heartCount,SaveManager.Instance.GetMaxHeartAmount());
                }
                _heartText.text=heartCount.ToString();
            }
        }
        protected override void DoOnClick()
        {
            if (!_isClickAble)
            {
                return;
            }
            base.DoOnClick();

            heartPanel.SetActive(true);
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