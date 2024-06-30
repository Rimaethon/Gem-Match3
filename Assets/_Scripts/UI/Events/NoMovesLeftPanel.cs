using Rimaethon.Scripts.Managers;
using TMPro;
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
        [SerializeField] private GameObject tryAgainPanel;
        [SerializeField] private GameObject butYouGonnaLoseThesePanel;
        [SerializeField] private GameObject eventProgressUIPrefab;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button playOnButton;
        bool isFirstPanel = true;
        private int movesToAdd= 5;
        private int _extraMoveCost = 900;
        private int _numberOfTimesAsked = 1;

        private void OnEnable()
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
            playOnButton.onClick.AddListener(OnContinueButtonClicked);
            addMovesCostText.text = (_extraMoveCost * _numberOfTimesAsked).ToString();
            isFirstPanel = true;
        }

        private void OnDisable()
        {
            playOnButton.onClick.RemoveListener(OnContinueButtonClicked);
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void OnContinueButtonClicked()
        {
            if( SaveManager.Instance.GetCoinAmount()< _extraMoveCost * _numberOfTimesAsked)
                return;

            SaveManager.Instance.AdjustCoinAmount(-_extraMoveCost * _numberOfTimesAsked);
            coinsText.text = SaveManager.Instance.GetCoinAmount().ToString();
            EventManager.Instance.Broadcast<int>(GameEvents.OnMoveCountChanged, movesToAdd);
            EventManager.Instance.Broadcast(GameEvents.OnBoardUnlock);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
            gameObject.SetActive(false);
            AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
            firstAskPanel.SetActive(true);
            butYouGonnaLoseThesePanel.SetActive(false);
            isFirstPanel = true;
            _numberOfTimesAsked++;
        }

        private void OnExitButtonClicked()
        {
            if (isFirstPanel)
            {
                firstAskPanel.SetActive(false);
                butYouGonnaLoseThesePanel.SetActive(true);
                AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
                isFirstPanel = false;
            }
            else
            {
                gameObject.SetActive(false);
                AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
                firstAskPanel.SetActive(true);
                butYouGonnaLoseThesePanel.SetActive(false);
                tryAgainPanel.SetActive(true);
                isFirstPanel = true;
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
