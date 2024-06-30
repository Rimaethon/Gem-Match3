using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class TryAgainPanel:MonoBehaviour
    {
        [SerializeField] private GameObject tryAgainPanel;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private List<GameObject> goalsFailedToCollect;
        
        private void Start()
        {
            int[] goalIds = SaveManager.Instance.GetCurrentLevelGoalIds();
            levelText.text = "Level " + SaveManager.Instance.GetCurrentLevelName();
            for(int i=0;i<goalIds.Length;i++)
            { 
                goalsFailedToCollect[i].SetActive(true);
                goalsFailedToCollect[i].GetComponent<Image>().sprite =
                    ObjectPool.Instance.GetItemSprite(goalIds[i]);
            }
        }
    }
}
