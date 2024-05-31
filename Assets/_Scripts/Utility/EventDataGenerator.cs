using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.Data_Classes;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class EventDataGenerator : MonoBehaviour
{
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private string eventPath = "Assets/Data/Events/";
    [SerializeField] private string eventName = "Event";
    [ValueDropdown("GetNormalItemIds")]
    [SerializeField] private int eventObjectiveSpriteID;
    [ValueDropdown("GetBoosterItemIds")]
    [SerializeField] private int eventRewardSpriteID;
    [SerializeField] private int rewardCount;
    [SerializeField] private bool isRewardUnlimitedUseForSpecificTime;
    [SerializeField] private int rewardUnlimitedUseTimeInSeconds;
    //Minute/Hour/Day/Month/Year Which will be converted to Unix Time
    [SerializeField] private long[] eventStartTimeM_H_D_M_Y=new long[5];
    //Minute/Hour/Day Which will be converted to seconds
    [SerializeField] private long[] eventDurationM_H_D=new long[3];
    [SerializeField] private int eventGoalCount;
    [SerializeField] private bool isMainEvent;
    private int _eventProgressCount=0;

    private IEnumerable<ValueDropdownItem<int>> GetNormalItemIds()
    {
        foreach (var item in itemDatabase.NormalItems)
        {
            yield return new ValueDropdownItem<int>(item.Value.ItemPrefab.name, item.Key);
        }
    }
    private IEnumerable<ValueDropdownItem<int>> GetBoosterItemIds()
    {
        foreach (var item in itemDatabase.Boosters)
        {
            yield return new ValueDropdownItem<int>(item.Value.ItemPrefab.name, item.Key);
        }
    }
    [Button]
    public void GenerateEventData()
    {
        DateTime dateTime = new DateTime((int)eventStartTimeM_H_D_M_Y[4], (int)eventStartTimeM_H_D_M_Y[3], (int)eventStartTimeM_H_D_M_Y[2], (int)eventStartTimeM_H_D_M_Y[1], (int)eventStartTimeM_H_D_M_Y[0], 0);

        EventData eventData = new EventData()
        {
            eventObjectiveID = eventObjectiveSpriteID,
            eventRewardID = eventRewardSpriteID,
            eventStartUnixTime = ((DateTimeOffset)dateTime).ToUnixTimeSeconds(),
            eventDuration = eventDurationM_H_D[0] * 60 + eventDurationM_H_D[1] * 3600 +
                                         eventDurationM_H_D[2] * 86400,
            eventProgressCount = _eventProgressCount,
            eventGoalCount = eventGoalCount,
            isMainEvent = isMainEvent, 
            rewardCount = rewardCount,
            isRewardUnlimitedUseForSpecificTime = isRewardUnlimitedUseForSpecificTime,
            rewardUnlimitedUseTimeInSeconds = rewardUnlimitedUseTimeInSeconds,
            
        
            
        };
        string path = eventPath + eventName + ".json";

        SaveToJson(path, eventData);
    }
    private void SaveToJson(string path, EventData eventData)
    {
        var serializedData = SerializationUtility.SerializeValue(eventData, DataFormat.JSON);
        File.WriteAllBytes(path, serializedData);
    }
}
