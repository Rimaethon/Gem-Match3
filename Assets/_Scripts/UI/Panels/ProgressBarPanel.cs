using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.Core.Interfaces;
using _Scripts.Data_Classes;
using _Scripts.UI.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Panels
{
	public class ProgressBarPanel:MonoBehaviour,ITimeDependent
	{
        [SerializeField] protected ItemDatabaseSO itemDatabase;
        [SerializeField] protected Image eventGoalIcon;
        [SerializeField] private TextMeshProUGUI eventRewardText;
        [SerializeField] protected Image eventRewardIcon;
        [SerializeField] protected Button button;
        [SerializeField] protected TextMeshProUGUI remainingTimeText;
        [SerializeField] private TextMeshProUGUI eventGoalProgressText;
        [SerializeField] private GameObject tapToClaimTextGameObject;
        [SerializeField] private Image fadeImage;
        [SerializeField] private Image eventProgressBar;
        [SerializeField] private List<EventRewardData> possibleRewards;
        protected EventRewardData _currentRewardData;
        protected EventData _eventData;
        protected const long EventDuration = 259200;
        private Vector3 _initialRewardIconPosition;
        protected AudioSource _audioSource;

        protected string _eventFolderPath = "Assets/Data/";
        protected string _eventRewardJsonName = "";
        protected string _eventJsonName = "";
        protected const string Extension = ".json";

        protected virtual void Awake()
        {
            InitializeReward();
            InitializeEventVisual();
        }


        protected virtual void OnExecute()
        {

        }
        private void InitializeReward()
        {
            if(File.Exists(_eventFolderPath+_eventRewardJsonName+Extension))
            {
                _currentRewardData = SaveManager.Instance.LoadFromJson<EventRewardData>(_eventFolderPath+_eventRewardJsonName+Extension);
            }
            else
            {
                _currentRewardData = possibleRewards[UnityEngine.Random.Range(0, possibleRewards.Count)];
                SaveManager.Instance.SaveToJson(_currentRewardData, _eventFolderPath+_eventRewardJsonName);
                Debug.LogError("File not found at path: "+_eventRewardJsonName);
            }

        }
        private void InitializeEventVisual()
        {
            eventRewardText.text = _currentRewardData.rewardAmount.ToString();
            eventRewardIcon.sprite =
                _currentRewardData.isRewardBooster? itemDatabase.GetBoosterSprite(_currentRewardData.eventRewardID):itemDatabase.GetItemSprite(_currentRewardData.eventRewardID);
            if(_currentRewardData.isRewardBooster && _currentRewardData.isRewardUnlimitedUseForSpecificTime)
            {
                eventRewardText.text = "∞ "+_currentRewardData.rewardUnlimitedUseTimeInSeconds/60+"m";
            }else
            {
                eventRewardText.text = "X"+_currentRewardData.rewardAmount;
            }
            eventGoalProgressText.text = _eventData.eventProgressCount + "/" + _currentRewardData.eventGoalAmount;
            eventProgressBar.fillAmount = (float) _eventData.eventProgressCount/ _currentRewardData.eventGoalAmount;

        }

        public virtual void OnTimeUpdate(long currentTime)
        {
            GetRemainingTime(currentTime);
        }
        private void GetRemainingTime(long currentTime)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime-_eventData.eventStartUnixTime + _eventData.eventDuration);
            if (timeSpan.TotalSeconds <= 0)
            {
                gameObject.SetActive(false);
                return;
            }
            string timeText;
            if (timeSpan.Days > 0)
            {
                timeText = $"{timeSpan.Days}d {timeSpan.Hours}h";
            }
            else if(timeSpan.Hours>0)
            {
                timeText = $"{timeSpan.Hours}h {timeSpan.Minutes}m";
            }else if(timeSpan.Minutes>0)
            {
                timeText = $"{timeSpan.Minutes}m";
            }
            else
            {
                timeText = $"{timeSpan.Seconds}s";
            }
            remainingTimeText.text = timeText;
        }

        protected async UniTask HandleProgressBar(int progressAmount)
        {
            int lastProgress=_eventData.eventProgressCount ;
            _eventData.eventProgressCount+=progressAmount;
            int excessAmount = _eventData.eventProgressCount - _currentRewardData.eventGoalAmount;
            if (excessAmount >= 0)
            {
                _eventData.eventProgressCount =_currentRewardData.eventGoalAmount ;
            }
            await IncreaseValueAndFillBar(lastProgress, 1f);
            if (excessAmount >= 0)
            {
                await HandleRewardWinAnimation();
                _eventData.eventProgressCount = excessAmount;
                await IncreaseValueAndFillBar(0, 1f);
            }
            SaveManager.Instance.SaveToJson(_eventData, _eventFolderPath+_eventJsonName);
        }
        private async UniTask IncreaseValueAndFillBar(int lastProgress, float duration)
        {
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.ProgressBarIncreaseSound,true);
            float t = 0;
            while (t < duration)
            {
                t += Time.fixedDeltaTime;
                eventGoalProgressText.text = (int)Mathf.Lerp(lastProgress,  _eventData.eventProgressCount, t) + "/" + _currentRewardData.eventGoalAmount;
                eventProgressBar.fillAmount = Mathf.Lerp((float)lastProgress / _currentRewardData.eventGoalAmount, (float)_eventData.eventProgressCount / _currentRewardData.eventGoalAmount, t);
                await UniTask.Delay(20);
            }
            _audioSource.Stop();
        }

        private async UniTask GetNewReward()
        {
            _currentRewardData= possibleRewards[UnityEngine.Random.Range(0, possibleRewards.Count)];
            SaveManager.Instance.SaveToJson(_currentRewardData, _eventRewardJsonName);
            InitializeEventVisual();
            eventRewardIcon.transform.position = Vector3.zero;
            eventRewardIcon.gameObject.SetActive(true);
            await eventRewardIcon.transform.DOScale(Vector3.one * 3, 0.5f).SetUpdate(UpdateType.Fixed).ToUniTask();
            await UniTask.WhenAll(eventRewardIcon.transform.DOMove(_initialRewardIconPosition, 0.4f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InSine).ToUniTask(),
                eventRewardIcon.transform.DOScale(Vector3.one, 0.4f).SetUpdate(UpdateType.Fixed).ToUniTask());
        }
        private async UniTask HandleRewardPopUp()
        {
            _initialRewardIconPosition = eventRewardIcon.transform.position;
            await eventRewardIcon.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetUpdate(UpdateType.Fixed);
            await UniTask.WhenAll(
                fadeImage.DOFade(0.95f, 1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutSine).ToUniTask(),
                eventRewardIcon.rectTransform.DOMove(Vector3.zero, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InSine)
                    .ToUniTask());
            tapToClaimTextGameObject.SetActive(true);
            var tcs = new UniTaskCompletionSource();
            void OnScreenTouchHandler(Vector2 touchPosition)
            {
                tcs.TrySetResult();
            }
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnScreenTouch, OnScreenTouchHandler);
            await tcs.Task;
            tapToClaimTextGameObject.SetActive(false);
            await  UniTask.WhenAll(eventRewardIcon.transform.DOMove(button.transform.position, 0.3f).SetUpdate(UpdateType.Fixed).ToUniTask(),fadeImage.DOFade(0, 0.3f).SetUpdate(UpdateType.Fixed).ToUniTask());
            eventRewardIcon.gameObject.SetActive(false);
            await button.transform.DOPunchScale(-Vector3.one * 0.05f, 0.5f).SetUpdate(UpdateType.Fixed)
                .SetEase(Ease.InSine).ToUniTask();
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnScreenTouch, OnScreenTouchHandler);
        }
        private async UniTask HandleRewardWinAnimation()
        {
            await HandleRewardPopUp();
            await GetNewReward();
            HandleRewardClaim();
        }
        private void HandleRewardClaim()
        {
            if(_currentRewardData.isRewardBooster)
            {
                if(_currentRewardData.isRewardUnlimitedUseForSpecificTime)
                {
                    SaveManager.Instance.AddTimeToUnlimitedBooster(_currentRewardData.eventRewardID, _currentRewardData.rewardUnlimitedUseTimeInSeconds);
                }
                else
                {
                    SaveManager.Instance.AdjustBoosterAmount(_currentRewardData.eventRewardID, _currentRewardData.rewardAmount);
                }
            }
            else if(_currentRewardData.isRewardCoin)
            {
                SaveManager.Instance.AdjustCoinAmount(_currentRewardData.rewardAmount);
            }
            else
            {
                SaveManager.Instance.AdjustPowerUpAmount(_currentRewardData.eventRewardID, _currentRewardData.rewardAmount);
            }
        }
	}
}
