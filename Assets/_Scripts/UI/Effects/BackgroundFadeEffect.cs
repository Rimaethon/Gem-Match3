using System;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    [RequireComponent(typeof(Image))]
    public class BackgroundFadeEffect:MonoBehaviour
    {
        private Image _image;
        
        private void Awake()
        {
            _image = GetComponent<Image>();
        }
        private void OnEnable()
        {
            EventManager.Instance.AddHandler(GameEvents.FadeBackground, HandleFade);
            EventManager.Instance.AddHandler(GameEvents.UnFadeBackground,HandleUnFade);
        }
        private void OnDisable()
        {
            if(EventManager.Instance == null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.FadeBackground, HandleFade);
            EventManager.Instance.RemoveHandler(GameEvents.UnFadeBackground,HandleUnFade);
        }
        private void HandleUnFade()
        {
            _image.DOFade(0, 0.2f);
        }

        private void HandleFade()
        {
            _image.DOFade(0.9f, 0.2f);
        }

    }
}