using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;

namespace _Scripts.UI.Events
{

    [Serializable]
    public class FortuneWheelEventData: EventData
    {
        public int userSpinAmount;
        public List<WheelElement> prizes;

    }
}
