using DG.Tweening;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class LevelCompletedPanel:MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private RectTransform starImageTransform;
        [SerializeField] private ParticleSystem starParticleSystem;
        [SerializeField] private GameObject _continueButtonGameObject;
        private Button _continueButton;

        private void OnEnable()
        {
            _continueButton = _continueButtonGameObject.GetComponentInChildren<Button>();
            _continueButton.gameObject.SetActive(true);
            _continueButton.enabled = false;
            levelText.text = "Level " + SaveManager.Instance.GetLevelIndex();
            starImageTransform.localScale = Vector3.one*3;
            starImageTransform.DORotate(new Vector3(0, 0, 360), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
            starImageTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).onComplete += () =>
            {
                EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
                _continueButton.enabled = true;
                starParticleSystem.Play();
                SaveManager.Instance.IncreaseLevelIndex();
            };
            _continueButton.onClick.AddListener(() =>
            {
                EventManager.Instance.Broadcast(GameEvents.OnReturnToMainMenu);
                gameObject.SetActive(false);
            });
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveAllListeners();

        }

    }
}
