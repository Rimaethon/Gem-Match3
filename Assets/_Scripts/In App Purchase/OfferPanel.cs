using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Purchasing.Core.BuyingConsumables
{
    public class OfferPanel : MonoBehaviour
    {
        
        public Button buyButton;
        public TextMeshProUGUI coinText;
        public TextMeshProUGUI boosterDurationText;
        public List<GameObject> boosterIconHolders;
        public List<GameObject> powerUpIconHolders;
        private Bundle _bundle;
        private ItemDatabaseSO _itemDatabase;
        
        public void InitializePanel(Bundle bundle,ItemDatabaseSO itemDatabase)
        {
            _bundle = bundle;
            _itemDatabase = itemDatabase;
            coinText.text = bundle.coinAmount.ToString();
            int unlimitedDuration = bundle.unlimitedDuration;
            if(bundle.unlimitedDuration/3600>0)
            {
                boosterDurationText.text = (unlimitedDuration/3600).ToString()+"h";
            }
            else if(unlimitedDuration/60>0)
            {
                boosterDurationText.text = (unlimitedDuration/60).ToString()+"m";
            }
            else
            {
                boosterDurationText.text = unlimitedDuration.ToString()+"s";
            }
            for (int i = 0; i < bundle.boosterPurchases.Count; i++)
            {
                boosterIconHolders[i].SetActive(true);
                boosterIconHolders[i].GetComponent<Image>().sprite = _itemDatabase.GetBoosterSprite(bundle.boosterPurchases[i]);
            }

            for (int i = 0; i < bundle.powerUpPurchases.Count; i++)
            {
                powerUpIconHolders[i].SetActive(true);
                powerUpIconHolders[i].GetComponent<Image>().sprite =_itemDatabase.GetBoosterSprite(bundle.powerUpPurchases[i].powerUpId);;
                powerUpIconHolders[i].GetComponentInChildren<TextMeshProUGUI>().text = "X"+bundle.powerUpPurchases[i].amount;
            }
        }
    
    }
}