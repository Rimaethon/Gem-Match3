using System.Collections.Generic;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI remainingMovesText;
    [SerializeField] private int moveCount;
    
    [SerializeField] List<GameObject> goals;
    private readonly List<Image> _goalSprites=new List<Image>();
    private readonly List<TextMeshProUGUI> _goalTexts=new List<TextMeshProUGUI>();
    private readonly List<GameObject> _goalCheckMarks=new List<GameObject>();
    private readonly Dictionary<int,int> _goalIndexDictionary=new Dictionary<int, int>();

    private void Awake()
    {
        remainingMovesText.text = moveCount.ToString();
    }
    private void OnEnable()
    {
        EventManager.Instance.AddHandler<Dictionary<int,List<Vector2Int>>>(GameEvents.OnGoalInitialization,InitializeGoals);
        EventManager.Instance.AddHandler(GameEvents.OnSuccessfulSwap, HandleRemainingMovesText);
        EventManager.Instance.AddHandler<int>(GameEvents.OnGoalDestroyed, HandleGoalDestruction);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<Dictionary<int,List<Vector2Int>>>(GameEvents.OnGoalInitialization,InitializeGoals);
        EventManager.Instance.RemoveHandler(GameEvents.OnSuccessfulSwap, HandleRemainingMovesText);
        EventManager.Instance.RemoveHandler<int>(GameEvents.OnGoalDestroyed, HandleGoalDestruction);
    }
    
    private void InitializeGoals(Dictionary<int,List<Vector2Int>> goalIDGoalCountDictionary)
    {

        foreach (GameObject goal in goals)
        {
            _goalSprites.Add(goal.GetComponent<Image>());
            _goalTexts.Add(goal.GetComponentInChildren<TextMeshProUGUI>());
            _goalCheckMarks.Add(goal.transform.GetChild(1).gameObject);
        }
        int index = 0;
        foreach (var goal in goalIDGoalCountDictionary)
        {
            _goalIndexDictionary.Add(index,goal.Value.Count);
            goals[index].SetActive(true);
            _goalSprites[index].sprite=ObjectPool.Instance.GetItemSprite(goal.Key);
            _goalTexts[index].text = goal.Value.Count.ToString();
            index++;
        }
    }
    private void HandleGoalDestruction(int itemID)
    {
        if(!_goalIndexDictionary.ContainsKey(itemID))
            return;
        _goalIndexDictionary[itemID]--;
        if (_goalIndexDictionary[itemID] <= 0)
        {
            _goalCheckMarks[_goalIndexDictionary[itemID]].SetActive(true);
            _goalTexts[_goalIndexDictionary[itemID]].text = "";
        }
        else
        {
            _goalTexts[_goalIndexDictionary[itemID]].text = _goalIndexDictionary[itemID].ToString();
        }
    }

    private void HandleRemainingMovesText()
    {
        moveCount--;
        remainingMovesText.text =moveCount.ToString(); 
        if(moveCount<=0)
            EventManager.Instance.Broadcast(GameEvents.OnNoMovesLeft);
    }

}
