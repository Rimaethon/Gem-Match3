using System;
using _Scripts.Data_Classes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Panels
{
    public class MainEventGoalObtainedPanel:MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleEffect;
        [SerializeField] private GameObject imageGameObject;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image fadeImage;
        private Image _image;
        private RectTransform _imageTransform;
        [SerializeField] private GameObject starBombEffect;
        private void Awake()
        {
            _image = imageGameObject.GetComponent<Image>();
            _imageTransform = imageGameObject.GetComponent<RectTransform>();
            particleEffect.gameObject.SetActive(false);
        }

        public async UniTask HandleMainEventGoalObtained(int count,Sprite sprite,Vector3 goalPos,GameObject reachedEffect=null)
        {
            var fadeImageTask = fadeImage.DOFade(0.6f, 0.5f).SetUpdate(UpdateType.Fixed).ToUniTask();
            _image.sprite = sprite;
            _imageTransform.localScale = Vector3.zero;
            imageGameObject.SetActive(true);
            countText.color = new Color(1, 1, 1, 0);
            var scaleImageTask = _imageTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutSine).ToUniTask();
            var countTextFadeInTask = countText.DOFade(1, 0.2f).SetUpdate(UpdateType.Fixed).ToUniTask();

            await UniTask.WhenAll(fadeImageTask, scaleImageTask, countTextFadeInTask);
            particleEffect.gameObject.SetActive(true);
            countText.gameObject.SetActive(true);
            countText.text = count.ToString();
            await UniTask.Delay(1000);
            particleEffect.gameObject.SetActive(false);

            var fadeImageOutTask = fadeImage.DOFade(0, 0.5f).SetUpdate(UpdateType.Fixed).ToUniTask();
            var moveImageTask = _imageTransform.transform.DOMove(goalPos, 0.5f).SetEase(Ease.InOutSine).ToUniTask();
            var countTextFadeOutTask = countText.DOFade(0, 0.2f).SetUpdate(UpdateType.Fixed).ToUniTask();
            var scaleImageDownTask = _imageTransform.DOScale(Vector3.one*0.5f, 0.5f).SetEase(Ease.InOutSine).ToUniTask();

            await UniTask.WhenAll(fadeImageOutTask, moveImageTask, countTextFadeOutTask, scaleImageDownTask);
            countText.gameObject.SetActive(false);
            imageGameObject.SetActive(false);
            PlayStarBombParticleEffect();

            await UniTask.WaitUntil(() => _imageTransform.transform.position == goalPos);
        }
        private void PlayStarBombParticleEffect()
        {
            ParticleSystem particleEffect = Instantiate(starBombEffect, _imageTransform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            particleEffect.Play();
        }
        
    }
}