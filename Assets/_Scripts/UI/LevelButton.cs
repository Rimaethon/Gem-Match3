using System;
using Data;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimaethon.Runtime.UI
{
    public class LevelButton: UIButton
    {
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] private Sprite greyButtonImage;
        
        private Button _button;
        private int _levelIndex=1;
        private const string LevelName = "Level ";
        private bool _isLevelExist;

        private void Start()
        {
            _button = GetComponent<Button>();
            if (levelText == null || _button == null)
            {
                Debug.LogError("Level Button is missing components");
                return;                
            }
            _levelIndex = SaveManager.Instance.GetLevelIndex();
            levelText.text = LevelName+_levelIndex;
            _isLevelExist = SaveManager.Instance.DoesLevelExist(_levelIndex);
            if (!_isLevelExist)
            {
                _button.GetComponent<Image>().sprite = greyButtonImage;
                levelText.color = Color.grey;
            }
        }

   
        protected override void DoOnClick()
        {
            if(!_isLevelExist)
                return;
            EventManager.Instance.Broadcast(GameEvents.OnLevelButtonPressed, _levelIndex);

        }
      
    }
}