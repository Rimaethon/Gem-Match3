using Data;
using Rimaethon.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace Rimaethon.Runtime.UI
{
    public class LevelButton: UIButton
    {
        private int levelIndex=1;
        [SerializeField] TextMeshProUGUI levelText;
        private string levelName="Level ";
        private void OnEnable()
        {
            EventManager.Instance.AddHandler<PlayerGameData>(GameEvents.OnDataInitialization, GetCurrentLevel);
        }
        private void OnDisable()
        {

            if(EventManager.Instance==null)return;
            EventManager.Instance.RemoveHandler<PlayerGameData>(GameEvents.OnDataInitialization, GetCurrentLevel);
        }

        private void GetCurrentLevel(PlayerGameData data)
        {
            levelIndex= data.Level;
            levelText.text = levelName + levelIndex;
        }
  

        protected override void DoOnClick()
        {
            EventManager.Instance.Broadcast(GameEvents.OnLevelButtonPressed, levelIndex);
        

        }
    }
}