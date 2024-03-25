using System.IO;
using DefaultNamespace;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts
{
    public class SceneController : PersistentSingleton<SceneController>
    {

        [SerializeField] private string filePath = "Assets/Data/Level Data/Level ";
        private LevelData _levelData;

    
        private void OnEnable()
        {
            EventManager.Instance.AddHandler<int>(GameEvents.OnLevelButtonPressed, SwitchToGameScene);
            SceneManager.sceneLoaded +=OnSceneLoaded;
            
            
        }

        
                
        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<int>(GameEvents.OnLevelButtonPressed, SwitchToGameScene);
        }

        private void SwitchToGameScene(int levelIndex)
        {
            //if(!File.Exists(filePath + levelIndex))return;
            
            //byte[] level = File.ReadAllBytes(filePath + levelIndex);
            //_levelData = SerializationUtility.DeserializeValue<LevelData>(level, DataFormat.JSON);
            SceneManager.LoadScene(1);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            int sceneIndex = scene.buildIndex;

            if (sceneIndex == 1)
            {
                EventManager.Instance.Broadcast(GameEvents.OnGameSceneLoaded);
                return;   
            }
            EventManager.Instance.Broadcast(GameEvents.OnMenuSceneLoaded);            

        }
    }
}
