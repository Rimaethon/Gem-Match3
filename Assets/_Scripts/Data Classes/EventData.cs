using System;
using Sirenix.Serialization;

namespace _Scripts.Data_Classes
{
    [Serializable]
    public class EventData
    {
        [OdinSerialize]
        public int EventObjectiveSpriteID { get; set; }
        [OdinSerialize]
        public int EventRewardSpriteID { get; set; }
        
        [OdinSerialize]
        public int[] EventStartTime { get; set; }
        [OdinSerialize]
        public int EventDuration { get; set; }
        
        [OdinSerialize]
        public int EventProgress { get; set; }
        [OdinSerialize]
        public int EventGoal { get; set; }
    }
}