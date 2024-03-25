using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class UIButton : MonoBehaviour,IPointerClickHandler
    {
        protected Button Button;

        protected virtual void Awake()
        {
            Button = GetComponent<Button>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            EventManager.Instance.Broadcast(GameEvents.OnButtonClick);
            DoOnClick();
        }

        protected virtual void DoOnClick()
        {
            
        }
    }
}