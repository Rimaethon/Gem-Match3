using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class NormalBlock:ItemBase
    {
        public override void OnMatch()
        {
            if (transform.gameObject.activeSelf == false)
            {
                return;
            }
                
            StartCoroutine(ScaleDownAndMatch());
        }

        private IEnumerator ScaleDownAndMatch()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = Vector3.zero; // Change this to the desired target scale

            float duration = 0.1f; // Change this to control the speed of the scaling
            float elapsed = 0f;

            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;

            base.OnMatch();
        }
        
    }
}