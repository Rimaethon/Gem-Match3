using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class EventGoalRemovalEffect:MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        private Image _image;
        private readonly Vector3 _upwardMovementAmount = new Vector3(0, 100, 0);
        private void Awake()
        {
            _image = GetComponent<Image>();
        }
        
        [Button]
        public void StartEffect(int itemID,int removalAmount,Vector3 position,bool isBooster)
        {
            _image.sprite = isBooster ? ObjectPool.Instance.GetBoosterSprite(itemID) : ObjectPool.Instance.GetItemSprite(itemID);
            _amountText.text = "+"+removalAmount.ToString();
            gameObject.GetComponent<RectTransform>().localPosition= position;
            _image.enabled = true;
            Debug.Log(gameObject.GetComponent<RectTransform>().position);
            gameObject.GetComponent<RectTransform>().DOLocalMove(gameObject.GetComponent<RectTransform>().localPosition+_upwardMovementAmount, 1f).SetUpdate(UpdateType.Fixed).OnComplete(() =>
            {
                _image.enabled = false;
                _amountText.text = "";
            });
        }
    }
}