using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class UserData
    {
        public int currentLevel=1;
        public int coinAmount=3000;
        public int heartAmount=5;
        public int starAmount=0;
        public bool hasUnlimitedHearts;
        public long unlimitedHeartStartTime;
        public long unlimitedHeartDuration;
        public int maxHeartAmount=5;
        //This value will store when first of 5(max) hearts is used 
        public long firstHeartNotBeingFullUnixTime;
        public int heartRefillTimeInSeconds=1200;
        public Dictionary<int, BoosterData> BoosterAmounts;
        public Dictionary<int, int> PowerUpAmounts;
        public bool isMusicOn=true;
        public bool isSfxOn=true;
        public bool isNotificationOn=true;
        public bool isHintOn=true;
    }
    [Serializable]
    public class BoosterData
    {
        public int boosterAmount;
        public bool isUnlimited;
        public long unlimitedStartTime;
        public long unlimitedDuration;

    }

}