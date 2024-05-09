using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace _Scripts.UI.Events
{
    public class FortuneWheelEvent:MonoBehaviour
    {
        [SerializeField] ItemDatabaseSO itemDatabase;
        [SerializeField] Canvas FortuneWheelCanvas;
        [SerializeField] Image EventGoalIcon;
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
        [SerializeField] private List<WheelElement> _prizes;
        [SerializeField] private List<EventData> _possibleRewards;
        private string _currentEventRewardDataPath = "Assets/Data/FortuneWheelEventData/FortuneWheelRewardEventData.json";
        private string _eventDataPath = "Assets/Data/FortuneWheelEventData/FortuneWheelEventData.json";
        private EventData _currentRewardData;
        private FortuneWheelEventData _eventData;
        List<float> cumulativeChances = new List<float>();
        private float totalChance;
        private float angleBetweenPrizes=51.5f;
        private bool isSpinning;
        private long eventDuration=60*60*24*3;
        private Vector3 _initialRewardIconPosition;
        
        
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
            _initialRewardIconPosition = eventRewardIcon.transform.position;
            EventGoalIcon.sprite = goalPrizeSprite;
            foreach (var prize in _prizes)
            {
                prize.PrizeImage.sprite = prize.IsExtraSpin ? extraSpinPrizeSprite : goalPrizeSprite;
                prize.PrizeAmountText.text = "X"+prize.Amount;
                totalChance += prize.Chance;
            }
            if(File.Exists(_currentEventRewardDataPath))
            {
                _currentRewardData = LoadFromJson<EventData>(_currentEventRewardDataPath);
            }
            else
            {
                _currentRewardData = _possibleRewards[UnityEngine.Random.Range(0, _possibleRewards.Count)];
                SaveToJson(_currentRewardData, _currentEventRewardDataPath);
                Debug.LogError("File not found at path: "+_currentEventRewardDataPath);
            }

            if (File.Exists(_eventDataPath))
            {
                _eventData = LoadFromJson<FortuneWheelEventData>(_eventDataPath);
            }
            else
            {
                _eventData = new FortuneWheelEventData()
                {
                    userSpinAmount = 3,
                    eventStartTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    eventEndTime = DateTimeOffset.Now.ToUnixTimeSeconds() + eventDuration
                };
                SaveToJson(_eventData, _eventDataPath);
            }
            HandleRemainingTime();
            remainingSpinText.text = ":" + _eventData.userSpinAmount;
            Debug.Log("Current Reward: " + _eventData.userSpinAmount);
            
            eventRewardText.text = _currentRewardData.rewardCount.ToString();
            eventRewardIcon.sprite =
                _currentRewardData.isRewardBooster? ObjectPool.Instance.GetBoosterSprite(_currentRewardData.eventRewardID):ObjectPool.Instance.GetItemSprite(_currentRewardData.eventRewardID);
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
            foreach (var prize in _prizes)
            {
                cumulativeChance += (prize.Chance / totalChance) * 100;
                cumulativeChances.Add(cumulativeChance);
            }
        }
        private void ChangeAmount()
        {
            _eventData.userSpinAmount++;
            remainingSpinText.text = ":" + _eventData.userSpinAmount;
        }
        private void HandleRemainingTime()
        {
            TimeSpan remainingTime = TimeSpan.FromSeconds(_eventData.eventEndTime - DateTimeOffset.Now.ToUnixTimeSeconds());
            int days = remainingTime.Days;
            int hours = remainingTime.Hours;
            int minutes = remainingTime.Minutes;
            remainingTimeText.text = $"{days}d {hours}h {minutes}m";
        }
        private void SpinWheel()
        {
            if (isSpinning||_eventData.userSpinAmount<=0)
            {
                return;
            }
            isSpinning = true;
            _eventData.userSpinAmount--;
            SaveToJson(_eventData, _eventDataPath);
            remainingSpinText.text = ":" + _eventData.userSpinAmount;
            float randomChance = UnityEngine.Random.Range(0, 100);
            int prizeIndex = 0;
            for (int i = 0; i < cumulativeChances.Count; i++)
            {
                if (randomChance <= cumulativeChances[i])
                {
                    prizeIndex = i;
                    break;
                }
            }
            float angleToPrize = prizeIndex * angleBetweenPrizes;
            float randomAngle = 720 + angleToPrize;

            wheelTransform.DORotate(new Vector3(0, 0, randomAngle), 2f, RotateMode.FastBeyond360)
                .SetUpdate(UpdateType.Fixed).SetEase(Ease.OutQuint).onComplete += () =>
            {
                CalculateReward().Forget();
            };
        }
        //Based on my tests it gets the job done but this logic is prone to error.
        private async UniTask CalculateReward()
        {
            int rotateIndex=(7-(int)Mathf.Abs((wheelTransform.rotation.eulerAngles.z+angleBetweenPrizes/2)%360/angleBetweenPrizes))%7;
            
            if (_prizes[rotateIndex].IsExtraSpin)
            {
                _eventData.userSpinAmount+=_prizes[rotateIndex].Amount;
                remainingSpinText.text = ":" + _eventData.userSpinAmount;
                isSpinning = false;
                SaveToJson(_eventData, _eventDataPath);
                return;
            }
            int lastProgress=_currentRewardData.eventProgressCount ;
            _currentRewardData.eventProgressCount+= _prizes[rotateIndex].Amount;
            int excessAmount = _currentRewardData.eventProgressCount - _currentRewardData.eventGoalCount;
            if (excessAmount >= 0)
            {
                _currentRewardData.eventProgressCount =_currentRewardData.eventGoalCount ;
            }
            SaveToJson(_currentRewardData, _currentEventRewardDataPath);
            for (int i = 0; i < 8; i++)
            {
                GameObject newWheel = Instantiate(horseshoePrefab,FortuneWheelCanvas.transform);
                newWheel.transform.position =_prizes[rotateIndex].PrizeImage.transform.position;
                newWheel.GetComponent<RectTransform>().DOMove(EventGoalIcon.transform.position, 0.5f).SetUpdate(UpdateType.Fixed).onComplete += () =>
                {
                    Destroy(newWheel);
                };
                await UniTask.Delay(TimeSpan.FromSeconds(0.1));
            }
            AudioSource audioSource=AudioManager.Instance.PlaySFX(SFXClips.ProgressBarIncreaseSound,true);

            float t = 0;
            eventProgressBar.DOFillAmount((float) _currentRewardData.eventProgressCount/ _currentRewardData.eventGoalCount, 1f).SetUpdate(UpdateType.Fixed).onUpdate += () =>
            {
                t += Time.fixedDeltaTime;
                eventGoalProgressText.text = (int)Mathf.Lerp(lastProgress,  _currentRewardData.eventProgressCount, t) + "/" + _currentRewardData.eventGoalCount;
                if(t>=1)
                {
                    audioSource.Stop();
                }
            };
            if(excessAmount<0)
            {
                isSpinning = false;
                return;
            }
            
            await eventRewardIcon.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetUpdate(UpdateType.Fixed);
            fadeImage.DOFade(0.95f, 1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutSine);
            await eventRewardIcon.transform.DOMove(Vector3.zero, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InSine).ToUniTask();
            tapToClaimTextGameObject.SetActive(true);
            Debug.Log("Reward Claimed");            
            var tcs = new UniTaskCompletionSource();
            // Add a handler to the OnScreenTouch event that completes the UniTaskCompletionSource.
            void OnScreenTouchHandler(Vector2 touchPosition)
            {
                tcs.TrySetResult();
            }
           
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnScreenTouch, OnScreenTouchHandler);
            await tcs.Task;
            eventRewardIcon.transform.DOMove(spinButton.transform.position, 0.3f).SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.InSine).onComplete
                += () =>
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
                    spinButton.transform.DOScale(Vector3.one * 0.95f, 0.3f).SetUpdate(UpdateType.Fixed)
                        .SetEase(Ease.InSine).onComplete += () =>
                    {
                        spinButton.transform.DOScale(Vector3.one, 0.3f).SetUpdate(UpdateType.Fixed)
                            .SetEase(Ease.OutSine);
                    };
                    tapToClaimTextGameObject.SetActive(false);
                    eventRewardIcon.transform.position = _initialRewardIconPosition;
                    eventRewardIcon.transform.localScale = Vector3.one;
                };
            fadeImage.DOFade(0, 0.7f).SetUpdate(UpdateType.Fixed);
            _currentRewardData= _possibleRewards[UnityEngine.Random.Range(0, _possibleRewards.Count)];
            SaveToJson(_currentRewardData, _currentEventRewardDataPath);
            eventRewardText.text = _currentRewardData.rewardCount.ToString();
            eventRewardIcon.sprite =
                _currentRewardData.isRewardBooster? ObjectPool.Instance.GetBoosterSprite(_currentRewardData.eventRewardID):ObjectPool.Instance.GetItemSprite(_currentRewardData.eventRewardID);
            if (_currentRewardData.isRewardBooster && _currentRewardData.isRewardUnlimitedUseForSpecificTime)
            {
                eventRewardText.text = "∞" + _currentRewardData.rewardUnlimitedUseTimeInSeconds / 60 + "m";
            }
            else
            {
                eventRewardText.text = "X" + _currentRewardData.rewardCount;
            }
            audioSource=AudioManager.Instance.PlaySFX(SFXClips.ProgressBarIncreaseSound,true);
            _currentRewardData.eventProgressCount+=excessAmount;
            eventProgressBar.fillAmount = 0;
            t = 0;
            eventProgressBar.DOFillAmount((float) _currentRewardData.eventProgressCount/ _currentRewardData.eventGoalCount, 1f).SetUpdate(UpdateType.Fixed).onUpdate += () =>
            {
                t += Time.fixedDeltaTime;
                eventGoalProgressText.text = (int)Mathf.Lerp(lastProgress,  _currentRewardData.eventProgressCount, t) + "/" + _currentRewardData.eventGoalCount;
                if(t>=1)
                {
                    audioSource.Stop();
                }
            };
            await UniTask.Delay(3000);
           
            isSpinning = false;
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnScreenTouch, OnScreenTouchHandler);
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

    [Serializable]
    public class FortuneWheelEventData
    {
        public int userSpinAmount;
        public long eventStartTime;
        public long eventEndTime;
    }
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