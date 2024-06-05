using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class QuitLevelPanel:MonoBehaviour
    {
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _closePanelButton;
        [SerializeField] private GameObject tryAgainPanel;
        
        private void OnEnable()
        {
            _quitButton.onClick.AddListener(HandleQuitButton);
            _closePanelButton.onClick.AddListener(HandleClosePanelButton);
        }

        private void HandleClosePanelButton()
        {
            gameObject.SetActive(false);
        }

        private void HandleQuitButton()
        {
            gameObject.SetActive(false);
            tryAgainPanel.SetActive(true);
        }
    }
}