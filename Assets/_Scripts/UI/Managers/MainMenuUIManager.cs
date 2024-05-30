using System;
using _Scripts.UI.Panels;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        [SerializeField] private RectTransform upperPanel;
        [SerializeField] private RectTransform lowerPanel;
        [SerializeField] private RectTransform leftEventPanel;
        [SerializeField] private RectTransform rightEventPanel;
        [SerializeField] private int upperPanelYEnd;
        [SerializeField] private int upperPanelYStart;
        [SerializeField] private int lowerPanelYEnd;
        [SerializeField] private int lowerPanelYStart;
        [SerializeField] private int leftEventPanelXEnd;
        [SerializeField] private int leftEventPanelXStart;
        [SerializeField] private int rightEventPanelXEnd;
        [SerializeField] private int rightEventPanelXStart;
        [SerializeField] private int xStrechtAmount;
        [SerializeField] private int yStrechtAmount;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] private RectTransform coinAndStarSpawnPosition;
        [SerializeField] private CoinResourceBar coinResourceBar;
        [SerializeField] private StarResourceBar starResourceBar;
        [SerializeField] private GameObject starBombParticleEffect;
        [SerializeField] private float duration;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private MainEventPanel mainEventPanel;
        private GraphicRaycaster _graphicRaycaster;
        private GameObject starEffect;
        private Transform starTransform;
        private Vector3 start;
        private Vector3 end;
        private float time = 0f;

        private void Awake()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        private void OnDisable()
        {
            _graphicRaycaster.enabled = false;
        }

        private async void Start()
        {
            //This line took a while to figure out
            Canvas.ForceUpdateCanvases();
            await HandleCoolUIAnimation();
            _graphicRaycaster.enabled = true;
            if(!SceneController.Instance.IsLevelCompleted)
                return;
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
            InitializeStarEffect();
            await MoveStarEffectToTarget();
            await UniTask.Delay(1000);
            await mainEventPanel.HandleMainEventGoalObtained();
            await UniTask.Delay(1000);
            EventManager.Instance.Broadcast(GameEvents.OnEventCurrencyAmountChanged);
            await UniTask.Delay(1000);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
        }

        //It is so obvious that UI needs a script for adjustment for these elements so that they look the same on different screen sizes
        private async UniTask HandleCoolUIAnimation()
        {
            upperPanel.anchoredPosition = new Vector2(upperPanel.anchoredPosition.x, upperPanelYStart);
            lowerPanel.anchoredPosition = new Vector2(lowerPanel.anchoredPosition.x, lowerPanelYStart);
            leftEventPanel.anchoredPosition = new Vector2(leftEventPanelXStart, leftEventPanel.anchoredPosition.y);
            rightEventPanel.anchoredPosition = new Vector2(rightEventPanelXStart, rightEventPanel.anchoredPosition.y);
            await UniTask.Delay(200);
            upperPanel.DOAnchorPosY(upperPanelYEnd - yStrechtAmount, 0.2f).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Fixed).onComplete += () =>
            {
                upperPanel.DOAnchorPosY(upperPanelYEnd, 0.1f).SetUpdate(UpdateType.Fixed);
            };
            lowerPanel.DOAnchorPosY(lowerPanelYEnd + yStrechtAmount, 0.2f).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Fixed).onComplete += () =>
            {
                lowerPanel.DOAnchorPosY(lowerPanelYEnd, 0.1f).SetUpdate(UpdateType.Fixed);
            };
            await UniTask.Delay(200);
            leftEventPanel.DOAnchorPosX(leftEventPanelXEnd + xStrechtAmount, 0.2f).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Fixed).onComplete += () =>
            {
                leftEventPanel.DOAnchorPosX(leftEventPanelXEnd, 0.1f).SetUpdate(UpdateType.Fixed);
            };
            rightEventPanel.DOAnchorPosX(rightEventPanelXEnd - xStrechtAmount, 0.2f).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Fixed).onComplete += () =>
            {
                rightEventPanel.DOAnchorPosX(rightEventPanelXEnd, 0.1f).SetUpdate(UpdateType.Fixed);
            };
            await UniTask.Delay(300);
        }
        
        
        private void InitializeStarEffect()
        {
            starEffect = Instantiate(itemDatabase.starParticleEffect, coinAndStarSpawnPosition.position, Quaternion.identity);
            starTransform = starEffect.gameObject.transform;
            start = starTransform.position;
            end = starResourceBar.starIconPosition.position;
        }

        private async UniTask MoveStarEffectToTarget()
        {
            while (time <= duration)
            {
                time += Time.fixedDeltaTime;
                UpdateStarEffectPositionAndRotation();
                await UniTask.Yield();
            }
            PlayStarBombParticleEffect();
            SaveManager.Instance.AdjustStarAmount(1);
            Destroy(starEffect);
        }

        private void UpdateStarEffectPositionAndRotation()
        {
            float remainingTime = duration - time;
            starTransform.rotation = remainingTime < 0.01f ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 0f, remainingTime * 360f / duration);
            float linearT = time / duration;
            float widthT = animationCurve.Evaluate(linearT);
            float width = Mathf.Lerp(0f, Screen.width * 0.001f, widthT);
            starTransform.position = Vector3.Lerp(start, end, linearT)+new Vector3(width,0f,0f);
        }
        private void PlayStarBombParticleEffect()
        {
            ParticleSystem particleEffect = Instantiate(starBombParticleEffect, starTransform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            particleEffect.Play();
        }
    }
}