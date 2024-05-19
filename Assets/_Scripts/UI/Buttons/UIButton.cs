using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    // just make things look clicky and I don't care if they do anything or not
    public class UIButton : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IPointerUpHandler
    {
        protected Button Button;
        protected Vector3 OriginalScale=>Vector3.one;
        protected Tween ScaleDownTween;
        protected Tween ScaleUpTween;
        protected RectTransform RectTransform;
        protected virtual void Awake()
        {
            Button = GetComponent<Button>();
            RectTransform = GetComponent<RectTransform>();
        }
        protected virtual void OnEnable()
        {
            transform.localScale = OriginalScale;
        }
        protected virtual void OnDisable()
        {
            ScaleDownTween.Kill();
            ScaleUpTween.Kill();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if(!Button.isActiveAndEnabled)
                return;
            DoOnClick();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if(!Button.isActiveAndEnabled)
                return;
            DoOnPointerDown();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if(!Button.isActiveAndEnabled)
                return;
            DoOnPointerUp();
        }
        protected virtual void DoOnClick()
        {
            ScaleDownTween.Kill();
            ScaleUpTween.Kill();
            AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
            RectTransform.localScale = OriginalScale;
        }
        protected virtual void DoOnPointerDown()
        {
            ScaleDownTween=RectTransform.DOScale(OriginalScale * 0.95f, 0.075f).SetUpdate(UpdateType.Fixed);
        }
        protected virtual void DoOnPointerUp()
        {
            ScaleUpTween=RectTransform.DOScale(OriginalScale, 0.075f).SetUpdate(UpdateType.Fixed);
        }
    }
}