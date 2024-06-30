using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : Singleton<InGameUIManager>
{
    [SerializeField] private TextMeshProUGUI remainingMovesText;
    [SerializeField] private RectTransform boosterAndSettingsPanel;
    [SerializeField] private RectTransform movesAndGoalsPanel;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private Transform goalParent;
    [SerializeField] private float stretchDuration = 0.5f;
    [SerializeField] private float snapDuration = 0.15f;
    private int _moveCount;
    private List<GameObject> _goals=new List<GameObject>();
    private List<Image> _goalSprites=new List<Image>();
    private List<TextMeshProUGUI> _goalTexts=new List<TextMeshProUGUI>();
    private List<GameObject> _goalCheckMarks=new List<GameObject>();
    private Dictionary<int,int> _goalIndexDictionary=new Dictionary<int, int>();
    private Dictionary<int, int> _goalIDGoalCountDictionary;
    private const int MovesAndGoalsYStart = 900;
    private const int MovesAndGoalsYEnd = 410;
    private const int BoosterAndSettingsYStart = -370;
    private const int BoosterAndSettingsYEnd = -150;
    private const int StretchAmount = 100;

    private bool isDisabled;

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<int>(GameEvents.OnMoveCountChanged, HandleRemainingMovesText);
        EventManager.Instance.AddHandler<int,int>(GameEvents.OnGoalUIUpdate, HandleGoalDestruction);
        EventManager.Instance.AddHandler(GameEvents.OnLevelFailed,HandleLevelFailed);
    }
    private void OnDisable()
    {
        isDisabled = true;
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<int>(GameEvents.OnMoveCountChanged, HandleRemainingMovesText);
        EventManager.Instance.RemoveHandler<int,int>(GameEvents.OnGoalUIUpdate, HandleGoalDestruction);
        EventManager.Instance.RemoveHandler(GameEvents.OnLevelFailed,HandleLevelFailed);
    }
    public Vector3 GetGoalPosition(int goalID)
    {
        if(!_goalIndexDictionary.ContainsKey(goalID))
            return Vector3.zero;
        return _goals[_goalIndexDictionary[goalID]].transform.position;
    }

    public async UniTask  HandleGoalAndPowerUpUI(Dictionary<int, int> goalIDGoalCountDictionary, int moveCount)
    {
        if(isDisabled)
            return;
        _goalIDGoalCountDictionary = goalIDGoalCountDictionary;
        _moveCount = moveCount;
        InitializeGaolsAndMoveCount();
        boosterAndSettingsPanel.anchoredPosition = new Vector2(boosterAndSettingsPanel.anchoredPosition.x, BoosterAndSettingsYStart);
        movesAndGoalsPanel.anchoredPosition = new Vector2(movesAndGoalsPanel.anchoredPosition.x, MovesAndGoalsYStart);
        await UniTask.Delay(200);
        await UniTask.WhenAll(
            movesAndGoalsPanel.DOAnchorPosY(MovesAndGoalsYEnd - StretchAmount, stretchDuration).SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Fixed).ToUniTask(),
            boosterAndSettingsPanel.DOAnchorPosY(BoosterAndSettingsYEnd + StretchAmount, stretchDuration).SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Fixed).ToUniTask());
        await UniTask.WhenAll(
            movesAndGoalsPanel.DOAnchorPosY(MovesAndGoalsYEnd, snapDuration).SetUpdate(UpdateType.Fixed).ToUniTask(),
            boosterAndSettingsPanel.DOAnchorPosY(BoosterAndSettingsYEnd, snapDuration).SetUpdate(UpdateType.Fixed)
                .ToUniTask());
    }
    private void InitializeGaolsAndMoveCount()
    {
        remainingMovesText.text = _moveCount.ToString();
        _goalIndexDictionary.Clear();
        _goals.Clear();
        _goals=new List<GameObject>();
        int index = 0;
        foreach (KeyValuePair<int, int> keyValuePair in _goalIDGoalCountDictionary)
        {
            GameObject goal = Instantiate(goalPrefab, goalParent);
            _goals.Add(goal);
            _goalSprites.Add(goal.GetComponent<Image>());
            _goalTexts.Add(goal.GetComponentInChildren<TextMeshProUGUI>());
            _goalCheckMarks.Add(goal.transform.GetChild(1).gameObject);
            _goalIndexDictionary.Add(keyValuePair.Key,index);
            _goals[index].SetActive(true);
            _goalSprites[index].sprite=ObjectPool.Instance.GetItemSprite(keyValuePair.Key);
            _goalTexts[index].text = keyValuePair.Value.ToString();
            index++;
        }
    }
    private void HandleGoalDestruction(int itemID,int goalCount)
    {
        if(!_goalIndexDictionary.ContainsKey(itemID))
            return;
        if (goalCount <= 0)
        {
            _goalCheckMarks[_goalIndexDictionary[itemID]].SetActive(true);
            _goalTexts[_goalIndexDictionary[itemID]].text = "";
        }
        else
        {
            _goalTexts[_goalIndexDictionary[itemID]].text = goalCount.ToString();
        }
    }
    private void HandleLevelFailed()
    {

    }
    private void HandleRemainingMovesText(int valueToAdd)
    {
        _moveCount+=valueToAdd;
        if(_moveCount>=0)
            remainingMovesText.text =_moveCount.ToString();
    }

}
