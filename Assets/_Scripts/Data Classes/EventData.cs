using System;
namespace _Scripts.Data_Classes 
{
    [Serializable]
    public class EventData
    {
        public int eventObjectiveID;
        public int eventRewardID;
        public bool isRewardBooster;
        public bool isRewardCoin;
        public int rewardCount;
        //Come on I can give better names
        public bool isRewardUnlimitedUseForSpecificTime;
        public int rewardUnlimitedUseTimeInSeconds;
        public long eventStartUnixTime;
        public long eventDuration;
        public int eventGoalCount;
        public bool isMainEvent;
        public int eventProgressCount;
    }
}