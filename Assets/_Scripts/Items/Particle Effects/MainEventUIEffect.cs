using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Scripts
{
    public class MainEventUIEffect : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Image image;
        public TextMeshProUGUI textMeshProUGUI;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();

        }

        public void Move()
        {
            rectTransform.DOMoveY(rectTransform.position.y + 0.5f, 0.6f).SetUpdate(UpdateType.Fixed).OnComplete(() =>
            {
                ObjectPool.Instance.ReturnMainEventUIEffect(this);
            });
        }
    }
}