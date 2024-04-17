using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class UIButton : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IPointerUpHandler
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
        public void OnPointerDown(PointerEventData eventData)
        {
            DoOnPointerDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DoOnPointerUp();
        }

        protected virtual void DoOnClick()
        {
            
        }
        protected virtual void DoOnPointerDown()
        {
            transform.DOScale(Vector3.one * 0.95f, 0.1f);

        }
        protected virtual void DoOnPointerUp()
        {
            transform.DOScale(Vector3.one, 0.1f);

        }

    
    }
}