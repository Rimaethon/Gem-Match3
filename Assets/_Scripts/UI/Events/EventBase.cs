using _Scripts.Core.Interfaces;
using UnityEngine;

namespace _Scripts.UI.Events
{
    public class EventBase: MonoBehaviour,ITimeDependent
    {
        public void OnTimeUpdate(long currentTime)
        {
            
        }
        
    }
}