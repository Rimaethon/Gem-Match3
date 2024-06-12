﻿using System;
using _Scripts.Managers;
using Data;
using Rimaethon.Scripts.Managers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class NoMovesLeftPanel:MonoBehaviour
    {
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text movesToOffer;
        [SerializeField] private TMP_Text addMovesExplanationText;
        [SerializeField] private TMP_Text addMovesCostText;
        [SerializeField] private GameObject firstAskPanel;
        //Perfect naming right ? :D
        [SerializeField] private GameObject butYouGonnaLoseThesePanel;
        [SerializeField] private GameObject eventProgressUIPrefab;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button playOnButton;
        bool isFirstPanel = true;
        private int movesToAdd= 5;
        private int cost = 900;
        private int numberOfTimesAsked = 1;

        private void OnEnable()
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
            playOnButton.onClick.AddListener(OnContinueButtonClicked);
            addMovesCostText.text = (cost * numberOfTimesAsked).ToString();
            isFirstPanel = true;
        }

        private void OnDisable()
        {
            playOnButton.onClick.RemoveListener(OnContinueButtonClicked);
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }
        
        private void OnContinueButtonClicked()
        {
            Debug.Log("Continue button clicked"+SaveManager.Instance.GetCoinAmount());
            if( SaveManager.Instance.GetCoinAmount()< cost * numberOfTimesAsked)
                return;
            
            SaveManager.Instance.AdjustCoinAmount(-cost * numberOfTimesAsked);
            coinsText.text = SaveManager.Instance.GetCoinAmount().ToString();
            EventManager.Instance.Broadcast<int>(GameEvents.OnMoveCountChanged, movesToAdd);
            gameObject.SetActive(false);
        }

        private void OnExitButtonClicked()
        {
            if (isFirstPanel)
            {
                firstAskPanel.SetActive(false);
                butYouGonnaLoseThesePanel.SetActive(true); 
                isFirstPanel = false;
            }
            else
            {
                gameObject.SetActive(false);
                firstAskPanel.SetActive(true);
                butYouGonnaLoseThesePanel.SetActive(false);
            }
        }

        private void Awake()
        {
            coinsText.text = SaveManager.Instance.GetCoinAmount().ToString();
            movesToOffer.text = movesToAdd.ToString();
            addMovesExplanationText.text = "Add " + movesToAdd + " moves to keep playing!";
   
            
        }

    }
}