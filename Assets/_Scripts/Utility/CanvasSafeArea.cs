using UnityEngine;

namespace Scripts
{
   public class CanvasSafeArea : MonoBehaviour
   {
      private RectTransform _rectTransform;
      private Rect safeArea;
      private Vector2 minAnchor;
      private Vector2 maxAnchor;

      private void Awake()
      {
         _rectTransform = GetComponent<RectTransform>();
         safeArea = Screen.safeArea;
         minAnchor = safeArea.position;
         maxAnchor = safeArea.position + safeArea.size;
         minAnchor.x /= Screen.width;
         minAnchor.y = 0;
         maxAnchor.x /= Screen.width;
         maxAnchor.y /=  Screen.height;
         maxAnchor.y = Mathf.Max(0.96f, maxAnchor.y);
         _rectTransform.anchorMin = minAnchor;
         _rectTransform.anchorMax = maxAnchor;
      }


   }
}
