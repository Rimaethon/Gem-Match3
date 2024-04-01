using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace Data
{
    [Serializable]
    public class UserData
    {
        public int currentLevel=1;
        public int coinCount=1000;
        public int heartCount=5;
        public int starCount=0;
        public Dictionary<int, int> BoosterCounts;
    }
}