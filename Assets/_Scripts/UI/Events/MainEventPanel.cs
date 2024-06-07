using System;
using System.Collections.Generic;
using _Scripts.Core.Interfaces;
using _Scripts.Data_Classes;
using _Scripts.UI.Panels;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class MainEventPanel:MonoBehaviour,ITimeDependent
    {
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private Image _progressBar;
        [SerializeField] private Image _eventGoalIcon;
        [SerializeField] private Image _eventRewardIcon;
        [SerializeField] private TMP_Text _remainingTime;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text _eventRewardCount;
        [SerializeField] private MainEventGoalObtainedPanel mainEventGoalObtainedPanel;
        private Dictionary<int, int> collectedItems = new Dictionary<int, int>();
        private EventData _eventData;
        
        private void OnEnable()
        {
            if (SaveManager.Instance.HasMainEvent())
            {
                _eventData = SaveManager.Instance.GetMainEventData();
                InitializeEvent();
            }
        }
        
        public void OnTimeUpdate(long currentTime)
        {
            GetRemainingTime(currentTime);
        }
        public async UniTask HandleMainEventGoalObtained()
        {
            collectedItems=SceneController.Instance.CollectedItems;
            foreach (var item in collectedItems)
            {
                await  mainEventGoalObtainedPanel.HandleMainEventGoalObtained(item.Value, _itemDatabase.GetItemSprite(item.Key),_eventGoalIcon.transform.position);
                int lastProgress = _eventData.eventProgressCount;
                _eventData.eventProgressCount += item.Value;
                SaveManager.Instance.SaveMainEventData(_eventData);
                AudioSource audioSource=AudioManager.Instance.PlaySFX(SFXClips.ProgressBarIncreaseSound,true);
                float t = 0;
                _progressBar.DOFillAmount((float) _eventData.eventProgressCount/ _eventData.eventGoalCount, 1f).SetUpdate(UpdateType.Fixed).onUpdate += () =>
                {
                    t += Time.fixedDeltaTime;
                    progressText.text = (int)Mathf.Lerp(lastProgress,  _eventData.eventProgressCount, t) + "/" + _eventData.eventGoalCount;
                    if(t>=1)
                    {
                        audioSource.Stop();
                    }
                };
            }
        }
        private void InitializeEvent()
        {
            _eventGoalIcon.sprite = _itemDatabase.GetItemSprite(_eventData.eventObjectiveID);
            _eventRewardIcon.sprite = _itemDatabase.GetBoosterSprite(_eventData.eventRewardID);
            progressText.text = _eventData.eventProgressCount + "/" + _eventData.eventGoalCount;
            _progressBar.fillAmount = (float) _eventData.eventProgressCount / _eventData.eventGoalCount;
            if(_eventData.isRewardUnlimitedUseForSpecificTime)
            {
                _eventRewardCount.text = "∞ "+_eventData.rewardUnlimitedUseTimeInSeconds/60+"m";
            }
            else
            {
                _eventRewardCount.text ="X"+ _eventData.rewardCount.ToString();
            }
        }
        private void GetRemainingTime(long currentTime)
        {
            long remainingTime = _eventData.eventStartUnixTime + _eventData.eventDuration - currentTime;
            if (remainingTime <= 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
            string timeText;
            if (timeSpan.Days > 0)
            {
                // If remaining time is more than 24 hours, show days and hours
                timeText = $"{timeSpan.Days}d {timeSpan.Hours}h";
            }
            else
            {
                // If remaining time is less than 24 hours, show hours and minutes
                timeText = $"{timeSpan.Hours}h {timeSpan.Minutes}m";
            }
            _remainingTime.text = timeText;
        }

      
    }
}