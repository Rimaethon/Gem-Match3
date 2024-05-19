using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

namespace Samples.Purchasing.Core.BuyingConsumables
{
    public class InAppPurchasePanel : MonoBehaviour, IDetailedStoreListener
    {
        [SerializeField] private ItemDatabaseSO itemDatabase;
        private IStoreController _storeController; // The Unity Purchasing system.
        //Your products IDs. They should match the ids of your products in your store.
        [SerializeField] Button closePanelButton;
        [SerializeField] GameObject offerPanelPrefab;
        [SerializeField] List<Bundle> bundles;
        private List<OfferPanel> offerPanels=new List<OfferPanel>();

        private void OnEnable()
        {
            foreach (var bundle in bundles)
            {
                var offerPanel = Instantiate(offerPanelPrefab, transform).GetComponent<OfferPanel>();
                offerPanel.InitializePanel(bundle, itemDatabase);
                offerPanels.Add(offerPanel);
                offerPanel.buyButton.onClick.AddListener(() => BuyBundle(bundle));
            }
            closePanelButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            foreach (var offerPanel in offerPanels)
            {
                offerPanel.buyButton.onClick.RemoveAllListeners();
            }
            closePanelButton.onClick.RemoveAllListeners();
        }

    

        private void Start()
        {
            InitializePurchasing();
            
        }

        private void BuyBundle(Bundle bundle)
        {
            _storeController.InitiatePurchase(bundle.bundleId);
        }

        void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var bundle in bundles)
            {
                
                builder.AddProduct(bundle.bundleId, ProductType.Consumable);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("In-App Purchasing successfully initialized");
            _storeController = controller;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, null);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

            if (message != null)
            {
                errorMessage += $" More details: {message}";
            }

            Debug.Log(errorMessage);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            foreach (var bundle in bundles)
            {
                if (product.definition.id == bundle.bundleId)
                {
                    AddBundleToUserData(bundle);
                    break;
                }
            }
            return PurchaseProcessingResult.Complete;
        }
        private void AddBundleToUserData(Bundle bundle)
        {
            bundle.boosterPurchases.ForEach(boosterPurchase =>
            {
                SaveManager.Instance.AddTimeToUnlimitedBooster(boosterPurchase, bundle.unlimitedDuration);
            });
            bundle.powerUpPurchases.ForEach(powerUpPurchase =>
            {
                SaveManager.Instance.AdjustPowerUpAmount(powerUpPurchase.powerUpId, powerUpPurchase.amount);
            });
            SaveManager.Instance.AdjustCoinAmount(bundle.coinAmount);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
                $" Purchase failure reason: {failureDescription.reason}," +
                $" Purchase failure details: {failureDescription.message}");
        }
    }

    [Serializable]
    public class Bundle
    {
        public string bundleId;
        public List<int> boosterPurchases;
        public int unlimitedDuration;  
        public List<PowerUpPurchase> powerUpPurchases;
        public int coinAmount;
        public int price;
        public string bundleName;
    }
    [Serializable]
    public class PowerUpPurchase
    {
        public int powerUpId;
        public int amount;
    }
}
