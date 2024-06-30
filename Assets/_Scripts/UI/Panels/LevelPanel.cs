using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class LevelPanel:MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Sprite _noNewLevelSprite;
        [SerializeField] private Sprite _hasNewLevelSprite;
        [SerializeField] private Button _openLevelPanelButton;
        [SerializeField] private Button _startLevelButton;
        [SerializeField] private TextMeshProUGUI levelButtonText;
        [SerializeField] private TextMeshProUGUI levelBannerText;
        private const string LevelName = "Level ";
        private bool _hasNewLevel;
        private void OnEnable()
        {
            _startLevelButton.onClick.AddListener(InitializeLevel);
            _openLevelPanelButton.onClick.AddListener(OpenPanel);
            EventManager.Instance.AddHandler(GameEvents.OnLevelDownloaded, HandleLevelButton);
        }
        private void OnDisable()
        {
            _startLevelButton.onClick.RemoveAllListeners();
            _openLevelPanelButton.onClick.RemoveAllListeners();
            if(EventManager.Instance!=null)
                EventManager.Instance.RemoveHandler(GameEvents.OnLevelDownloaded, HandleLevelButton);
        }
        private void Awake()
        {
           HandleLevelButton();
        }
        private void HandleLevelButton()
        {
            _hasNewLevel = SaveManager.Instance.HasNewLevel();
            if (!_hasNewLevel)
            {
                levelButtonText.color = Color.grey;
                _openLevelPanelButton.gameObject.GetComponent<Image>().sprite = _noNewLevelSprite;
                levelButtonText.text = LevelName + SaveManager.Instance.GetCurrentLevelName();
            }
            else
            {
                levelButtonText.color = Color.white;
                _openLevelPanelButton.gameObject.GetComponent<Image>().sprite = _hasNewLevelSprite;
                levelButtonText.text = LevelName+SaveManager.Instance.GetCurrentLevelName();
                levelBannerText.text = LevelName+SaveManager.Instance.GetCurrentLevelName();
            }
        }
        private void InitializeLevel()
        {
            EventManager.Instance.Broadcast(GameEvents.OnLevelButtonPressed);
        }
        private void OpenPanel()
        {
            if(!_hasNewLevel)
                return;
            _panel.SetActive(true);
        }
        private void ClosePanel()
        {
            _panel.SetActive(false);
        }

    }
}
