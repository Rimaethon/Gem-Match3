using System;
using Sirenix.Serialization;

namespace _Scripts.Data_Classes
{
    //Definitely there is a better way to store date and time but for now this will do  
    [Serializable]
    public class HeartData
    {
        [OdinSerialize]
        public int HeartCount { get; set; }

        [OdinSerialize]
        public int HeartRegenTime { get; set; }
        
        [OdinSerialize]
        public int[] LastHeartLossTime { get; set; }
        //Just for the case if player goes offline exactly for 24 hours or 30~ days or 365~ days
        [OdinSerialize]
        public int[] LastHeartLossDate { get; set; }
        
    }
}