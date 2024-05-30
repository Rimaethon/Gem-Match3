using _Scripts.Items.ItemActions;
using _Scripts.Managers;
using Data;
using DG.Tweening;
using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InGamePowerUpPanel : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private UIBoosterButton hammerBooster;
    [SerializeField] private UIBoosterButton bowBooster;
    [SerializeField] private UIBoosterButton cannonBooster;
    [SerializeField] private UIBoosterButton jesterHatBooster;
    private UIBoosterButton _currentBooster=null;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject unFadeAbleSettingsButton;
    [SerializeField] private Image fadeEffect;
    [SerializeField] private Button jesterHatShuffleButton;
    [SerializeField] private RectTransform jesterHatVisualTransform;
    //Probably needs to be adjusted for different screen sizes
    private readonly float _jesterHatStartPos=2.7f;
    private readonly Vector3 _hammerRotationAmount = new Vector3(0, 0, -50);
    private bool _isAnyBoosterClicked;
    private void OnEnable()
    {
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnScreenTouch, CheckIfValidClick);
        hammerBooster.boosterButton.onClick.AddListener(() => HandleBoosterButtonClick(hammerBooster));
        bowBooster.boosterButton.onClick.AddListener(() => HandleBoosterButtonClick(bowBooster));
        cannonBooster.boosterButton.onClick.AddListener(() => HandleBoosterButtonClick(cannonBooster));
        jesterHatBooster.boosterButton.onClick.AddListener(() => HandleBoosterButtonClick(jesterHatBooster));
        jesterHatShuffleButton.onClick.AddListener(HandleShuffleButtonClick);
    
    }
    private void OnDisable()
    {
       hammerBooster.boosterButton.onClick.RemoveAllListeners();
       bowBooster.boosterButton.onClick.RemoveAllListeners();
       cannonBooster.boosterButton.onClick.RemoveAllListeners();
       jesterHatBooster.boosterButton.onClick.RemoveAllListeners();
       if (EventManager.Instance == null)
           return;
       EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnScreenTouch, CheckIfValidClick);
    }
    private void Awake()    
    {
        hammerBooster.boosterCounter.text = SaveManager.Instance.GetPowerUpAmount(hammerBooster.itemID).ToString();
        bowBooster.boosterCounter.text = SaveManager.Instance.GetPowerUpAmount(bowBooster.itemID).ToString();
        cannonBooster.boosterCounter.text = SaveManager.Instance.GetPowerUpAmount(cannonBooster.itemID).ToString();
        jesterHatBooster.boosterCounter.text =SaveManager.Instance.GetPowerUpAmount(jesterHatBooster.itemID).ToString();
    }
    private void CheckIfValidClick(Vector2 clickPos)
    {
        Vector2Int gridPos = LevelGrid.Instance.WorldToCellVector2Int(clickPos);
        //In the future this will be used for some kind of animation (maybe)
        bool isValid = LevelManager.Instance.IsValidPosition(gridPos);
        if (isValid)
        {
            HandleValidClick(gridPos);
        }else if (_currentBooster != null)
        {
            UnClickBooster(_currentBooster);
        }
    }
    private void HandleValidClick(Vector2Int gridPos)
    {
        if(_currentBooster==null)
            return;
        if(_currentBooster==hammerBooster)
        {
            HandleHammerPowerUp(gridPos);
            return;
        }
        if(_currentBooster==bowBooster)
        {
           HandleBowPowerUp(gridPos);
            return;
        }
        if(_currentBooster==cannonBooster)
        {
            HandleCannonPowerUp(gridPos);
            return;
        }
    }

    private void HandleCannonPowerUp(Vector2Int gridPos)
    {
        EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
        UnClickBooster(_currentBooster);
        EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, gridPos,cannonBooster.itemID,0);
    }


    private void HandleBowPowerUp(Vector2Int gridPos)
    {
        EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
        UnClickBooster(_currentBooster);
        EventManager.Instance.Broadcast(GameEvents.AddActionToHandle, gridPos,bowBooster.itemID,0);
    }

    private void HandleShuffleButtonClick()
    {
        jesterHatVisualTransform.gameObject.SetActive(true);
        EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
        jesterHatBooster.boosterPanel.SetActive(false);
        fadeEffect.DOFade(0,0.1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
        {
            fadeEffect.raycastTarget = true;
        };
        unFadeAbleSettingsButton.SetActive(true);
        jesterHatVisualTransform.DOMoveX(0, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
            () =>
            {
                jesterHatVisualTransform.DOMoveX(0, 0.8f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
                    () =>
                    {
                        EventManager.Instance.Broadcast(GameEvents.OnShuffleBoard);
                        SaveManager.Instance.AdjustPowerUpAmount(jesterHatBooster.itemID, -1);
                        jesterHatBooster.boosterCounter.text = SaveManager.Instance.GetPowerUpAmount(jesterHatBooster.itemID).ToString();
                            jesterHatVisualTransform.DOMoveX(_jesterHatStartPos, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
                            () =>
                            {
                                jesterHatVisualTransform.gameObject.SetActive(false);
                                EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
                                EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
                                jesterHatVisualTransform.position = new Vector3(-_jesterHatStartPos, jesterHatVisualTransform.position.y, jesterHatVisualTransform.position.z);
                                unFadeAbleSettingsButton.SetActive(false);
                            };
                    };
            };
    }

    private void HandleHammerPowerUp(Vector2Int gridPos)
    {
        if(!hammerBooster.isClicked)
            return;
        EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
        Vector3 worldPos =LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
        worldPos.x -= 0.25f;
        worldPos.y += 0.10f;
        GameObject hammerObject=ObjectPool.Instance.GetBoosterGameObject(hammerBooster.itemID,worldPos);
        hammerObject.transform.DORotate(-1.25f*_hammerRotationAmount, 0.8f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
            () =>
            {
                hammerObject.transform.DORotate(_hammerRotationAmount, 0.3f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
                    () =>
                    {
                        EventManager.Instance.Broadcast(GameEvents.OnBoardShake);
                        LevelManager.Instance.CheckHammerHit(gridPos);
                        SaveManager.Instance.AdjustPowerUpAmount(hammerBooster.itemID, -1);
                        hammerBooster.boosterCounter.text = SaveManager.Instance.GetPowerUpAmount(hammerBooster.itemID).ToString();;
                        Destroy(hammerObject);
                        EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
                    };
            };
        HandleBoosterButtonClick(hammerBooster);
    }
    
       
    private void HandleBoosterButtonClick(UIBoosterButton booster)
    {
        if (booster.isClicked)
        {
            UnClickBooster(booster);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock); 
            _isAnyBoosterClicked = false;
            booster.isClicked = false;
        }
        else if(!_isAnyBoosterClicked)
        {
            if (SaveManager.Instance.GetPowerUpAmount(booster.itemID) <= 0)
                return;
            HandlePowerUpClick(booster);
            
            _isAnyBoosterClicked = true;
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
        }
        else if(_currentBooster!=null)
        {
            Debug.Log("Current Booster is not null");
            _currentBooster.isClicked = false;
            _isAnyBoosterClicked = false;
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock); 
            UnClickBooster(_currentBooster);
        }
        
    }
    private void UnClickBooster(UIBoosterButton booster)
    {
        EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
        canvas.sortingOrder = 40;
        booster.isClicked = false;
        _isAnyBoosterClicked = false;
        booster.clickedCounter.SetActive(false);
        booster.unClickedCounter.SetActive(true);
        booster.boosterPanel.SetActive(false);
        fadeEffect.DOFade(0f, 0.1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
        {
            fadeEffect.raycastTarget = false;
        };
        booster.boosterTransform.SetAsFirstSibling();
        _currentBooster = null;
    }
    private void ClickBooster(UIBoosterButton booster)
    {
        EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
        canvas.sortingOrder = 0;
        booster.isClicked = true;
        booster.unClickedCounter.SetActive(false);
        booster.clickedCounter.SetActive(true);
        unFadeAbleSettingsButton.SetActive(false);
        fadeEffect.DOFade(0.9f,0.1f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
        {
            fadeEffect.raycastTarget = true;
        };
        fadeEffect.gameObject.transform.SetAsLastSibling();
        booster.boosterPanel.SetActive(true);
        booster.boosterTransform.SetAsLastSibling();
        _currentBooster = booster;
    }
    private void HandlePowerUpClick(UIBoosterButton booster)
    {
       UnClickBooster(hammerBooster);
       UnClickBooster(bowBooster);
       UnClickBooster(cannonBooster);
       UnClickBooster(jesterHatBooster);
        ClickBooster(booster);
    }
}
