using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

//Canvases are messed up in order to accommodate fading system. Maybe my future self will look at this and laugh.
public class InGameSettingsButton : MonoBehaviour
{
    [SerializeField] private GameObject _unFadeAbleSettingsButton;
    [SerializeField] private Button _settingsOpenerButton;
    [SerializeField] private Button _settingsCloserButton;
    [SerializeField] private RectTransform iconTransform;
    [SerializeField] private GameObject _settingsFade;
    [SerializeField] private Button _sfxButton;
    [SerializeField] private GameObject  _sfxDisabledIcon;
    [SerializeField] private Button _musicButton;
    [SerializeField] private GameObject _musicDisabledIcon;
    [SerializeField] private Button _exitButton;
    [SerializeField] private GameObject _quitLevelPanel;
    [SerializeField] private RectTransform soundButtonTransform;
    [SerializeField] private RectTransform musicButtonTransform;
    [SerializeField] private RectTransform exitButtonTransform;
    private readonly Vector3 _rotationAmount = new Vector3(0, 0, 120);
    private  float _buttonMoveAmount = 2.7f;
    private bool _isClicked;
    private bool isAnyButtonClicked;
    private bool isMusicOn;
    private bool isSfxOn;
    private WaitForSeconds _waitForButtonClick=new WaitForSeconds(0.2f);
    private void OnEnable()
    {
        _settingsOpenerButton.interactable = true;
        _settingsCloserButton.interactable = false;
        _settingsOpenerButton.onClick.AddListener( OpenSettings);
        _settingsCloserButton.onClick.AddListener(CloseSettings);
        _exitButton.onClick.AddListener(() =>
        {
            isAnyButtonClicked = true;
            _quitLevelPanel.SetActive(true);
            _settingsFade.SetActive(false);
            musicButtonTransform.gameObject.SetActive(false);
            soundButtonTransform.gameObject.SetActive(false);
            exitButtonTransform.gameObject.SetActive(false);
        });
        _sfxButton.onClick.AddListener(() =>
        {
            isSfxOn = !isSfxOn;
            SaveManager.Instance.SetSFX(isSfxOn);
            _sfxDisabledIcon.SetActive(!isSfxOn);
            isAnyButtonClicked = true;
      
        });
        _musicButton.onClick.AddListener(() =>
        {
            isMusicOn = !isMusicOn;
            SaveManager.Instance.SetMusic(isMusicOn);
            _musicDisabledIcon.SetActive(!isMusicOn);
            isAnyButtonClicked = true;
        });
    }
    
    
    private void OnDisable()
    {
        _settingsOpenerButton.onClick.RemoveAllListeners();
        _settingsCloserButton.onClick.RemoveAllListeners();
        _exitButton.onClick.RemoveAllListeners();
        if(EventManager.Instance==null)
            return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnScreenTouch, HandleScreenTouch);
    }
    private void OpenSettings()
    {
        if (_isClicked) return;
        _isClicked = true;
        isAnyButtonClicked = true;
        
        musicButtonTransform.gameObject.SetActive(true);
        soundButtonTransform.gameObject.SetActive(true);
        exitButtonTransform.gameObject.SetActive(true);
        isMusicOn = SaveManager.Instance.IsMusicOn();
        isSfxOn = SaveManager.Instance.IsSfxOn();
        _musicDisabledIcon.SetActive(!isMusicOn);
        _sfxDisabledIcon.SetActive(!isSfxOn);
        _unFadeAbleSettingsButton.SetActive(true);
        _settingsFade.SetActive(true);
        HandleMovement(iconTransform.position.x, 0.08f, 0.12f,-0.07f);
        iconTransform.DOLocalRotate(iconTransform.rotation.eulerAngles+_rotationAmount, 0.4f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
        {
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnScreenTouch, HandleScreenTouch);
            _settingsOpenerButton.interactable = false;
            _settingsCloserButton.interactable = true;
        };
    }
    private bool isCoroutineRunning;
    private void HandleScreenTouch(Vector2 touchPosition)
    {
        if (isCoroutineRunning) return;
        isCoroutineRunning = true;
        StartCoroutine(CheckIfAnyButtonClicked());

    }
    private IEnumerator CheckIfAnyButtonClicked()
    {
        yield return _waitForButtonClick;
        if (isAnyButtonClicked)
        {
            isAnyButtonClicked = false;
        }
        else
        {
            CloseSettings();
        }
        isCoroutineRunning = false;
    }


    private void CloseSettings()
    {
        if (!_isClicked) return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnScreenTouch, HandleScreenTouch);
        _isClicked= false;
        isAnyButtonClicked = true;
        HandleMovement(_buttonMoveAmount, 0.07f, 0f);
        iconTransform.DOLocalRotate(iconTransform.rotation.eulerAngles - _rotationAmount, 0.15f)
            .SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
        {
            _unFadeAbleSettingsButton.SetActive(false);
            musicButtonTransform.gameObject.SetActive(false);
            soundButtonTransform.gameObject.SetActive(false);
            exitButtonTransform.gameObject.SetActive(false);
            _settingsOpenerButton.interactable = true;
            _settingsCloserButton.interactable = false;
            _settingsFade.SetActive(false);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
        };
     
    }
    private void HandleMovement(  float xPosition,float duration,float delay,float xOffset=0f)
    {
        soundButtonTransform.DOMoveX(xPosition + xOffset, duration).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutQuad)
            .onComplete += () =>
        {
            soundButtonTransform.DOMoveX(xPosition, duration).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
        };
        musicButtonTransform.DOMoveX(xPosition+xOffset, duration+delay).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutQuad).onComplete += () =>
        {
            musicButtonTransform.DOMoveX(xPosition, duration).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
        };
        exitButtonTransform.DOMoveX(xPosition+xOffset, duration+2*delay).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutQuad).onComplete += () =>
        {
            exitButtonTransform.DOMoveX(xPosition, duration).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
        };
    }
}
