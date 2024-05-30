using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.Core.Interfaces;
using _Scripts.Data_Classes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace _Scripts.UI.Events
{
    //This class was too complicated with big methods so I decided to split it into smaller methods.
    public class FortuneWheelEvent:MonoBehaviour,ITimeDependent
    {
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] private Canvas fortuneWheelCanvas;
        [SerializeField] private Image eventGoalIcon;
        [SerializeField] private Image extraSpinIcon;
        [SerializeField] private Button spinButton;
        [SerializeField] private RectTransform wheelTransform;
        [SerializeField] private TextMeshProUGUI eventRewardText;
        [SerializeField] private Image eventRewardIcon;
        [SerializeField] private Sprite goalPrizeSprite;
        [SerializeField] private Sprite extraSpinPrizeSprite;
        [SerializeField] private TextMeshProUGUI remainingSpinText;
        [SerializeField] private TextMeshProUGUI remainingTimeText;
        [SerializeField] private TextMeshProUGUI eventGoalProgressText;
        [SerializeField] private GameObject tapToClaimTextGameObject;
        [SerializeField] private Image fadeImage;
        [SerializeField] private Image eventProgressBar;
        [SerializeField] private GameObject horseshoePrefab;
        [SerializeField] private GameObject sherifStarPrefab;
        [SerializeField] private List<WheelElement> prizes;
        [SerializeField] private List<EventData> possibleRewards;
        [SerializeField] private TextMeshProUGUI buttonRemainingTimeText;
        private readonly string _currentEventRewardDataPath = "Assets/Data/FortuneWheelEventData/FortuneWheelRewardEventData.json";
        private const string EventDataPath = "Assets/Data/FortuneWheelEventData/FortuneWheelEventData.json";
        private EventData _currentRewardData;
        private FortuneWheelEventData _eventData;
        private readonly List<float> _cumulativeChances = new List<float>();
        private float _totalChance;
        private const float AngleBetweenPrizes = 51.5f;
        private bool _isSpinning;
        private const long EventDuration = 7500;
        private Vector3 _initialRewardIconPosition;
        private AudioSource _audioSource;
        
        private void OnEnable()
        {
            EventManager.Instance.AddHandler(GameEvents.OnEventCurrencyAmountChanged,ChangeAmount );
            spinButton.onClick.AddListener(SpinWheel);
        }
        private void OnDisable()
        {
            spinButton.onClick.RemoveListener(SpinWheel);
            if (EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnEventCurrencyAmountChanged, ChangeAmount);
        }
        private void Awake()
        {
            eventGoalIcon.sprite = goalPrizeSprite;
            foreach (var prize in prizes)
            {
                prize.PrizeImage.sprite = prize.IsExtraSpin ? extraSpinPrizeSprite : goalPrizeSprite;
                prize.PrizeAmountText.text = "X"+prize.Amount;
                _totalChance += prize.Chance;
            }
            if(File.Exists(_currentEventRewardDataPath))
            {
                _currentRewardData = LoadFromJson<EventData>(_currentEventRewardDataPath);
            }
            else
            {
                _currentRewardData = possibleRewards[UnityEngine.Random.Range(0, possibleRewards.Count)];
                SaveToJson(_currentRewardData, _currentEventRewardDataPath);
                Debug.LogError("File not found at path: "+_currentEventRewardDataPath);
            }

            if (File.Exists(EventDataPath))
            {
                _eventData = LoadFromJson<FortuneWheelEventData>(EventDataPath);
            }
            else
            {
                _eventData = new FortuneWheelEventData()
                {
                    userSpinAmount = 3,
                    eventStartTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    eventDuration = DateTimeOffset.Now.ToUnixTimeSeconds() + EventDuration
                };
                SaveToJson(_eventData, EventDataPath);
            }
            remainingSpinText.text = ":" + _eventData.userSpinAmount;
            eventRewardText.text = _currentRewardData.rewardCount.ToString();
            eventRewardIcon.sprite =
                _currentRewardData.isRewardBooster? itemDatabase.GetBoosterSprite(_currentRewardData.eventRewardID):itemDatabase.GetItemSprite(_currentRewardData.eventRewardID);
            if(_currentRewardData.isRewardBooster && _currentRewardData.isRewardUnlimitedUseForSpecificTime)
            {
                eventRewardText.text = "∞"+_currentRewardData.rewardUnlimitedUseTimeInSeconds/60+"m";
            }else 
            {
                eventRewardText.text = "X"+_currentRewardData.rewardCount;
            }
            eventGoalProgressText.text = _currentRewardData.eventProgressCount + "/" + _currentRewardData.eventGoalCount;
            eventProgressBar.fillAmount = (float) _currentRewardData.eventProgressCount/ _currentRewardData.eventGoalCount;
            float cumulativeChance = 0;
            foreach (var prize in prizes)
            {
                cumulativeChance += (prize.Chance / _totalChance) * 100;
                _cumulativeChances.Add(cumulativeChance);
            }
        }

        private void ChangeAmount()
        {
            _eventData.userSpinAmount++;
            remainingSpinText.text = ":" + _eventData.userSpinAmount;
        }
        public void OnTimeUpdate(long currentTime)
        {
            TimeSpan remainingTime = TimeSpan.FromSeconds(currentTime-_eventData.eventStartTime + _eventData.eventDuration);
            int days = remainingTime.Days;
            int hours = remainingTime.Hours;
            int minutes = remainingTime.Minutes;
            remainingTimeText.text = $"{days}d {hours}h {minutes}m";
            buttonRemainingTimeText.text = $"{days}d {hours}h {minutes}m";
        }
        private void SpinWheel()
        {
            if (_isSpinning||_eventData.userSpinAmount<=0)
            {
                return;
            }
            _initialRewardIconPosition = eventRewardIcon.transform.position;
            _isSpinning = true;
            _eventData.userSpinAmount--;
            SaveToJson(_eventData, EventDataPath);
            remainingSpinText.text = ":" + _eventData.userSpinAmount;
            float randomChance = UnityEngine.Random.Range(0, 100);
            int prizeIndex = 0;
            for (int i = 0; i < _cumulativeChances.Count; i++)
            {
                if (randomChance <= _cumulativeChances[i])
                {
                    prizeIndex = i;
                    break;
                }
            }
            float angleToPrize = prizeIndex * AngleBetweenPrizes;
            float randomAngle = 720 + angleToPrize;

            wheelTransform.DORotate(new Vector3(0, 0, randomAngle), 2f, RotateMode.FastBeyond360)
                .SetUpdate(UpdateType.Fixed).SetEase(Ease.OutQuint).onComplete += () =>
            {
                CalculateReward().Forget();
            };
        }
        private async UniTask CalculateReward()
        {
            int rotateIndex=(7-(int)Mathf.Abs((wheelTransform.rotation.eulerAngles.z+AngleBetweenPrizes/2)%360/AngleBetweenPrizes))%7;
            
            if (prizes[rotateIndex].IsExtraSpin)
            {
                await HandleExtraSpinReward(rotateIndex, prizes[rotateIndex].Amount);
                _isSpinning = false;
                SaveToJson(_eventData, EventDataPath);
                return;
            }
            await UniTask.WhenAll( HandleProgressBar(prizes[rotateIndex].Amount), MoveHorseshoe(rotateIndex, 8));
            SaveToJson(_currentRewardData, _currentEventRewardDataPath);
            _isSpinning = false;
        }
        private async UniTask MoveHorseshoe(int rotateIndex, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject newWheel = Instantiate(horseshoePrefab,fortuneWheelCanvas.transform);
                newWheel.transform.position =prizes[rotateIndex].PrizeImage.transform.position;
                newWheel.GetComponent<RectTransform>().DOMove(eventGoalIcon.transform.position, 0.5f).SetUpdate(UpdateType.Fixed).onComplete += () =>
                {
                    Destroy(newWheel);
                };
                await UniTask.Delay(100);
            }
        }

        private async UniTask HandleExtraSpinReward(int rotateIndex, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject newWheel = Instantiate(sherifStarPrefab,fortuneWheelCanvas.transform);
                newWheel.transform.position =prizes[rotateIndex].PrizeImage.transform.position;
                newWheel.GetComponent<RectTransform>().DOMove(extraSpinIcon.transform.position, 0.5f).SetUpdate(UpdateType.Fixed).onComplete += () =>
                {
                    _eventData.userSpinAmount++;
                    remainingSpinText.text = ":" + _eventData.userSpinAmount;
                    Destroy(newWheel);
                };
                await UniTask.Delay(100);
            }
        }
    
        private async UniTask HandleProgressBar(int progressAmount)
        {
            int lastProgress=_currentRewardData.eventProgressCount ;
            _currentRewardData.eventProgressCount+=progressAmount;
            int excessAmount = _currentRewardData.eventProgressCount - _currentRewardData.eventGoalCount;
            if (excessAmount >= 0)
            {
                _currentRewardData.eventProgressCount =_currentRewardData.eventGoalCount ;
            }
            await IncreaseValueAndFillBar(lastProgress, 1f);
            if (excessAmount >= 0)
            {
                await HandleRewardWinAnimation();
                _currentRewardData.eventProgressCount = excessAmount;
                await IncreaseValueAndFillBar(0, 1f);
            }
        }
        private async UniTask IncreaseValueAndFillBar(int lastProgress, float duration)
        {
            _audioSource=AudioManager.Instance.PlaySFX(SFXClips.ProgressBarIncreaseSound,true);
            float t = 0;
            while (t < duration)
            {
                t += Time.fixedDeltaTime;
                eventGoalProgressText.text = (int)Mathf.Lerp(lastProgress,  _currentRewardData.eventProgressCount, t) + "/" + _currentRewardData.eventGoalCount;
                eventProgressBar.fillAmount = Mathf.Lerp((float)lastProgress / _currentRewardData.eventGoalCount, (float)_currentRewardData.eventProgressCount / _currentRewardData.eventGoalCount, t);
                await UniTask.Delay(20);
            }
            _audioSource.Stop();
        }
        
        private async UniTask GetNewReward()
        {
            _currentRewardData= possibleRewards[UnityEngine.Random.Range(0, possibleRewards.Count)];
            SaveToJson(_currentRewardData, _currentEventRewardDataPath);
            eventRewardIcon.sprite =
                _currentRewardData.isRewardBooster? itemDatabase.GetBoosterSprite(_currentRewardData.eventRewardID):itemDatabase.GetItemSprite(_currentRewardData.eventRewardID);
            if (_currentRewardData.isRewardBooster && _currentRewardData.isRewardUnlimitedUseForSpecificTime)
            {
                eventRewardText.text = "∞" + _currentRewardData.rewardUnlimitedUseTimeInSeconds / 60 + "m";
            }
            else
            {
                eventRewardText.text = "X" + _currentRewardData.rewardCount;
            }
            
            eventRewardIcon.transform.position = Vector3.zero;
            eventRewardIcon.gameObject.SetActive(true);
            await eventRewardIcon.transform.DOScale(Vector3.one * 3, 0.5f).SetUpdate(UpdateType.Fixed).ToUniTask();
            await UniTask.WhenAll(eventRewardIcon.transform.DOMove(_initialRewardIconPosition, 0.4f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InSine).ToUniTask(),
                eventRewardIcon.transform.DOScale(Vector3.one, 0.4f).SetUpdate(UpdateType.Fixed).ToUniTask());
        }
        private async UniTask HandleRewardPopUp()
        {
            await eventRewardIcon.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetUpdate(UpdateType.Fixed);
            await UniTask.WhenAll(
                fadeImage.DOFade(0.95f, 1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutSine).ToUniTask(),
                eventRewardIcon.transform.DOMove(Vector3.zero, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InSine)
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
            await  UniTask.WhenAll(eventRewardIcon.transform.DOMove(spinButton.transform.position, 0.3f).SetUpdate(UpdateType.Fixed).ToUniTask(),fadeImage.DOFade(0, 0.3f).SetUpdate(UpdateType.Fixed).ToUniTask());
            eventRewardIcon.gameObject.SetActive(false);
            await spinButton.transform.DOPunchScale(-Vector3.one * 0.05f, 0.5f).SetUpdate(UpdateType.Fixed)
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
                    SaveManager.Instance.AdjustBoosterAmount(_currentRewardData.eventRewardID, _currentRewardData.rewardCount);
                }
            }
            else if(_currentRewardData.isRewardCoin)
            {
                SaveManager.Instance.AdjustCoinAmount(_currentRewardData.rewardCount);
            }
            else
            {
                SaveManager.Instance.AdjustPowerUpAmount(_currentRewardData.eventRewardID, _currentRewardData.rewardCount);
            }
        }
        private void SaveToJson<T>(T data, string path)
        {
            var serializedData = SerializationUtility.SerializeValue(data, DataFormat.JSON);
            File.WriteAllBytes(path , serializedData);
        }
        private T LoadFromJson<T>(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return SerializationUtility.DeserializeValue<T>(bytes, DataFormat.JSON);
        }

      
    }


}