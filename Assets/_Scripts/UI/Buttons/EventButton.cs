using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace _Scripts.UI.Buttons
{
    public class EventButton:CanvasChangerButton
    {
        [SerializeField] private Sprite eventCurrencySprite;
        [SerializeField] private GameObject eventCurrencyPrefab;
        [SerializeField] private RectTransform eventCurrencySpawnPoint;
        [SerializeField] private GameObject starBombParticleEffect;
        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.AddHandler(GameEvents.OnEventCurrencyAmountChanged, UpdateEventCurrencyAmount);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnEventCurrencyAmountChanged, UpdateEventCurrencyAmount);
        }
        
        private void UpdateEventCurrencyAmount()
        {
            GameObject eventCurrency = Instantiate(eventCurrencyPrefab, gameObject.transform);
            eventCurrency.GetComponent<Image>().sprite = eventCurrencySprite;
            eventCurrency.transform.position = eventCurrencySpawnPoint.position;
            eventCurrency.transform.DOLocalJump(gameObject.transform.position, 150, 1, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                GameObject effect=Instantiate(starBombParticleEffect, gameObject.transform);
                effect.transform.position = gameObject.transform.position;
                    gameObject.transform.DOPunchScale(Vector3.one*-0.1f, 0.3f, 0, 0).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                    {
                        Destroy(effect);
                        Destroy(eventCurrency);
                    });
                });
        }

    }
}