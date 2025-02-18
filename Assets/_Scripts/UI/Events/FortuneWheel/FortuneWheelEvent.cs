using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.UI.Panels;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace _Scripts.UI.Events
{
    //This class was too complicated with big methods so I decided to split it into smaller methods.
    public class FortuneWheelEvent:ProgressBarPanel
    {
        [SerializeField] private Canvas fortuneWheelCanvas;
        [SerializeField] private Image extraSpinIcon;
        [SerializeField] private RectTransform wheelTransform;
        [SerializeField] private Sprite goalPrizeSprite;
        [SerializeField] private Sprite extraSpinPrizeSprite;
        [SerializeField] private TextMeshProUGUI remainingSpinText;
        [SerializeField] private GameObject horseshoePrefab;
        [SerializeField] private GameObject sherifStarPrefab;
        [SerializeField] private List<WheelElement> prizes;
        [SerializeField] private TextMeshProUGUI buttonRemainingTimeText;
        private FortuneWheelEventData _fortuneWheelEventData;
        private readonly List<float> _cumulativeChances = new List<float>();
        private float _totalChance;
        private const float AngleBetweenPrizes = 51.5f;
        private bool _isSpinning;
    Nullable<int> someInt = null;
        private void OnEnable()
        {
            EventManager.Instance.AddHandler(GameEvents.OnEventCurrencyAmountChanged, ChangeAmount);
            button.onClick.AddListener(SpinWheel);
        }
        private void OnDisable()
        {
            button.onClick.RemoveListener(SpinWheel);
            if (EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnEventCurrencyAmountChanged, ChangeAmount);
        }
        protected override void Awake()
        {
            eventGoalIcon.sprite = goalPrizeSprite;
            _eventFolderPath = "Assets/Data/FortuneWheelEventData/";
            _eventRewardJsonName = "FortuneWheelRewardEventData";
            _eventJsonName = "FortuneWheelEventData";
            InitializeEventData();
            base.Awake();
            InitializePrizes();
            remainingSpinText.text = ":" + _fortuneWheelEventData.userSpinAmount;
        }
        private void InitializeEventData()
        {
            if (File.Exists(_eventFolderPath+_eventJsonName+Extension))
            {
                _fortuneWheelEventData = SaveManager.Instance.LoadFromJson<FortuneWheelEventData>(_eventFolderPath+_eventJsonName+Extension);
            }
            else
            {
                _fortuneWheelEventData = new FortuneWheelEventData()
                {
                    userSpinAmount = 3,
                    eventStartUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    eventDuration = DateTimeOffset.Now.ToUnixTimeSeconds() + EventDuration
                };
                SaveManager.Instance.SaveToJson(_fortuneWheelEventData, _eventFolderPath+_eventJsonName);
            }
            _eventData= _fortuneWheelEventData;
        }
        private void InitializePrizes()
        {
            float cumulativeChance = 0;

            foreach (var prize in prizes)
            {
                prize.PrizeImage.sprite = prize.IsExtraSpin ? extraSpinPrizeSprite : goalPrizeSprite;
                prize.PrizeAmountText.text = "X"+prize.Amount;
                _totalChance += prize.Chance;
            }
            foreach (var prize in prizes)
            {
                cumulativeChance += (prize.Chance / _totalChance) * 100;
                _cumulativeChances.Add(cumulativeChance);
            }

        }

        private void ChangeAmount()
        {
            _fortuneWheelEventData.userSpinAmount++;
            remainingSpinText.text = ":" + _fortuneWheelEventData.userSpinAmount;
        }
        public override void OnTimeUpdate(long currentTime)
        {
            base.OnTimeUpdate(currentTime);
            buttonRemainingTimeText.text = remainingTimeText.text;
        }
        private void SpinWheel()
        {
            if (_isSpinning||_fortuneWheelEventData.userSpinAmount<=0)
            {
                return;
            }
            _isSpinning = true;
            _fortuneWheelEventData.userSpinAmount--;
            SaveManager.Instance.SaveToJson(_fortuneWheelEventData, _eventFolderPath+_eventJsonName);
            remainingSpinText.text = ":" + _fortuneWheelEventData.userSpinAmount;
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
                await UniTask.Delay(400);
                _isSpinning = false;
                SaveManager.Instance.SaveToJson(_fortuneWheelEventData, _eventFolderPath+_eventJsonName);
                return;
            }
            await UniTask.WhenAll( HandleProgressBar(prizes[rotateIndex].Amount), MoveHorseshoe(rotateIndex, 8));
            await UniTask.Delay(400);
            SaveManager.Instance.SaveToJson(_currentRewardData, _eventFolderPath+_eventRewardJsonName);
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
                    _fortuneWheelEventData.userSpinAmount++;
                    remainingSpinText.text = ":" + _fortuneWheelEventData.userSpinAmount;
                    Destroy(newWheel);
                };
                await UniTask.Delay(100);
            }
        }



    }


}
