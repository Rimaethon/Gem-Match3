using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

//Canvases are messed up in order to accommodate fading system. Maybe my future self will look at this and laugh.
public class InGameSettingsButton : MonoBehaviour
{
    [SerializeField] private Button _settingsButton;
    [SerializeField] private RectTransform iconTransform;
    [SerializeField] private GameObject _settingsFade;
    [SerializeField] private Button _soundButton;
    [SerializeField] private Button _musicButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private GameObject _quitLevelPanel;
    [SerializeField] private RectTransform soundButtonTransform;
    [SerializeField] private RectTransform musicButtonTransform;
    [SerializeField] private RectTransform exitButtonTransform;
    private readonly Vector3 _rotationAmount = new Vector3(0, 0, 180);
    private  float _buttonMoveAmount = 2.7f;
    private bool _isClicked;
    private void OnEnable()
    {
        _settingsButton.onClick.AddListener( HandleClick);
        _exitButton.onClick.AddListener(() =>
        {
            _quitLevelPanel.transform.localScale = Vector3.zero;
            _quitLevelPanel.SetActive(true);
            _quitLevelPanel.transform.DOScale(Vector3.one, 0.1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InSine);
            _settingsFade.SetActive(false);
            musicButtonTransform.gameObject.SetActive(false);
            soundButtonTransform.gameObject.SetActive(false);
            exitButtonTransform.gameObject.SetActive(false);
            HandleClick();
        });
    }
    private void OnDisable()
    {
        _settingsButton.onClick.RemoveAllListeners();
    }
    private void HandleClick()
    {
        if (!_isClicked)
        {
            musicButtonTransform.gameObject.SetActive(true);
            soundButtonTransform.gameObject.SetActive(true);
            exitButtonTransform.gameObject.SetActive(true);
            HandleMovement(iconTransform.position.x, 0.15f, 0.10f);
            iconTransform.DOLocalRotate(iconTransform.rotation.eulerAngles-_rotationAmount, 0.35f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
            _settingsFade.SetActive(true);
            _isClicked = true;
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
        }
        else
        {
            HandleMovement(_buttonMoveAmount, 0.15f, 0.10f);
            iconTransform.DOLocalRotate(iconTransform.rotation.eulerAngles + _rotationAmount, 0.35f)
                .SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
            {
                musicButtonTransform.gameObject.SetActive(false);
                soundButtonTransform.gameObject.SetActive(false);
                exitButtonTransform.gameObject.SetActive(false);
            };
            _settingsFade.SetActive(false);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
            _isClicked= false;
        }
    }
    private void HandleMovement(  float xPosition,float duration,float delay)
    {
        soundButtonTransform.DOMoveX(xPosition, duration).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
        musicButtonTransform.DOMoveX(xPosition, duration+delay).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
        exitButtonTransform.DOMoveX(xPosition, duration+2*delay).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
    }
}
