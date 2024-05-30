using System;
using System.Collections.Generic;

namespace _Scripts.UI.Events
{
  
    [Serializable]
    public class FortuneWheelEventData
    {
        public int userSpinAmount;
        public long eventStartTime;
        public long eventDuration;
        public List<WheelElement> prizes;
        
    }
}