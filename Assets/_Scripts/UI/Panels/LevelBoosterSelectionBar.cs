using System;
using System.Collections.Generic;
using _Scripts.Core.Interfaces;
using Newtonsoft.Json.Converters;
using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace _Scripts.Items.ItemActions
{
    public class LevelBoosterSelectionBar:MonoBehaviour,ITimeDependent
    {
        [SerializeField] private List<UIBoosterButton> _boosters;
        [SerializeField] private Sprite _unclickedBackgroundSprite;
        [SerializeField] private Sprite _clickedBackgroundSprite;
        private void OnEnable()
        {
            EventManager.Instance.AddHandler(GameEvents.OnBoosterAmountChanged, HandleBoosterCounts);
            foreach (var booster in _boosters)
            {
                if (SaveManager.Instance.HasUnlimitedBooster(booster.itemID))
                {
                    booster.isClicked = true;
                    booster.unlimitedCounter.SetActive(false);
                    booster.unlimitedCounter.SetActive(true);
                    booster.unlimitedCounterText.text =SaveManager.Instance.GetUnlimitedBoosterTime(booster.itemID);
                    booster.boosterBackground.sprite = _clickedBackgroundSprite;
                    EventManager.Instance.Broadcast<int>(GameEvents.OnBoosterUsed,booster.itemID);
                }
                booster.boosterButton.onClick.AddListener(() =>
                {
                    if (SaveManager.Instance.HasUnlimitedBooster(booster.itemID))
                    {
                        EventManager.Instance.Broadcast<int>(GameEvents.OnBoosterUsed,booster.itemID);
                        return;
                    }
                    if(SaveManager.Instance.GetBoosterAmount(booster.itemID)<=0)
                        return;
                    EventManager.Instance.Broadcast<int>(GameEvents.OnBoosterButtonClicked,booster.itemID);
                    if(booster.isClicked)
                    {

                        booster.isClicked = false;
                        booster.boosterBackground.sprite = _unclickedBackgroundSprite;
                        booster.unClickedCounter.SetActive(true);
                        booster.clickedCounter.SetActive(false);
                        EventManager.Instance.Broadcast<int>(GameEvents.OnBoosterRemoved,booster.itemID);
                    }
                    else
                    {
                        booster.isClicked = true;
                        booster.boosterBackground.sprite = _clickedBackgroundSprite;
                        booster.unClickedCounter.SetActive(false);
                        booster.clickedCounter.SetActive(true);
                        EventManager.Instance.Broadcast<int>(GameEvents.OnBoosterUsed,booster.itemID);
                    }
                });
            }
            HandleBoosterCounts();
        }

        private void OnDisable()
        {
            foreach (var booster in _boosters)
            {
                booster.boosterButton.onClick.RemoveAllListeners();
            }
            if(EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnBoosterAmountChanged, HandleBoosterCounts);
        }


        private void HandleBoosterCounts()
        {
            foreach (var booster in _boosters)
            {
                if (SaveManager.Instance.HasUnlimitedBooster(booster.itemID))
                {
                    booster.boosterCounter.text = "∞";
                    continue;
                }

                booster.boosterCounter.text= SaveManager.Instance.GetBoosterAmount(booster.itemID).ToString();
            }
        }

        public void OnTimeUpdate(long currentTime)
        {
            foreach (var booster in _boosters)
            {
                if (SaveManager.Instance.HasUnlimitedBooster(booster.itemID))
                {
                    booster.unlimitedCounterText.text =SaveManager.Instance.GetUnlimitedBoosterTime(booster.itemID);
                }
            }
        }
    }
}
