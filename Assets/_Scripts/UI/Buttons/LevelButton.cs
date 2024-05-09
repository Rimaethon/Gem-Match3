using DG.Tweening;
using Rimaethon.Runtime.UI;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Buttons
{
    public class LevelButton:UIButton
    {
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] private Sprite greyButtonImage;
        private int _levelIndex=1;
        private const string LevelName = "Level ";
        private bool _isLevelExist;

        private void Start()
        {
            _levelIndex = SaveManager.Instance.GetLevelIndex();
            _isLevelExist = SaveManager.Instance.HasNewLevel();
            levelText.text = LevelName+_levelIndex;
            if (_isLevelExist) return;
            Button.GetComponent<Image>().sprite = greyButtonImage;
            levelText.color = Color.grey;
        }
        protected override void DoOnClick()
        {
            Debug.Log("Level Button Clicked");
            if(!_isLevelExist)
                return;
            base.DoOnClick();
            
            EventManager.Instance.Broadcast(GameEvents.OnLevelButtonPressed, _levelIndex);
        }
    }
}