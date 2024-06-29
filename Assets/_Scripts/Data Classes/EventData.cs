using System;
namespace _Scripts.Data_Classes
{
    [Serializable]
    public class EventData
    {
        public int eventGoalID;
        public long eventStartUnixTime;
        public long eventDuration;
        public int eventProgressCount;
    }

    [Serializable]
    public class EventRewardData
    {
        public bool isRewardUnlimitedUseForSpecificTime;
        public int rewardUnlimitedUseTimeInSeconds;
        public int eventRewardID;
        public bool isRewardBooster;
        public bool isRewardCoin;
        public int rewardAmount;
        public int eventGoalAmount;
    }
}
