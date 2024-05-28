using System.Collections.Generic;
using Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Panels
{
    public class ButYouGonnaLoseThesePanel:MonoBehaviour
    {
        [SerializeField] private GameObject progressGainedThisLevel;
        [SerializeField] private GameObject eventProgressUIPrefab;
        public Dictionary<int, int> itemsCollectedThisLevel;

        private void OnEnable()
        {
            itemsCollectedThisLevel = SceneController.Instance.CollectedItems;
            foreach (var item in itemsCollectedThisLevel)
            {
                GameObject progress = Instantiate(eventProgressUIPrefab, progressGainedThisLevel.transform);
                progress.GetComponent<Image>().sprite=ObjectPool.Instance.GetItemSprite(item.Key);
                progress.GetComponentInChildren<TextMeshProUGUI>().text = item.Value.ToString();
            }
        }
        
        
    }
}