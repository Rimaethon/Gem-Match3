using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Scripts.Managers;
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
        private LevelData _levelData;
        private readonly HashSet<int> _boostersUsedThisLevel=new HashSet<int>();
        public Dictionary<int,int> CollectedItems=new Dictionary<int, int>();

        public bool IsLevelCompleted
        {
            get;
            private set;
        }
        private void OnEnable()
        {
            EventManager.Instance.AddHandler(GameEvents.OnLevelButtonPressed, SwitchToGameScene);
            EventManager.Instance.AddHandler<int>(GameEvents.OnBoosterUsed, AddBoostersUsed);
            EventManager.Instance.AddHandler<int>(GameEvents.OnBoosterRemoved, RemoveBoostersUsed);
            EventManager.Instance.AddHandler<IBoardItem,int>(GameEvents.OnMainEventGoalRemoval, HandleMainEventGoalRemoved);
            EventManager.Instance.AddHandler(GameEvents.OnLevelFailed, () =>
            {
                CollectedItems.Clear();
                IsLevelCompleted = false;
                SceneManager.LoadScene(0);
            });
            EventManager.Instance.AddHandler(GameEvents.OnReturnToMainMenu, () =>
            {
                IsLevelCompleted = true;
                SceneManager.LoadScene(0);
            });
        }


        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnLevelFailed, () => { SceneManager.LoadScene(0); });
            EventManager.Instance.RemoveHandler<int>(GameEvents.OnBoosterUsed, AddBoostersUsed);
            EventManager.Instance.RemoveHandler<int>(GameEvents.OnBoosterRemoved, RemoveBoostersUsed);
            EventManager.Instance.RemoveHandler(GameEvents.OnLevelButtonPressed, SwitchToGameScene);
            EventManager.Instance.RemoveHandler<IBoardItem,int>(GameEvents.OnMainEventGoalRemoval, HandleMainEventGoalRemoved);
            EventManager.Instance.RemoveHandler(GameEvents.OnReturnToMainMenu, () =>
            {
                SceneManager.LoadScene(0);
            });
        }

        private void SwitchToGameScene()
        {
            SceneManager.LoadScene(1);
        }
        public List<int> GetBoostersUsedThisLevel()
        {
            if( _boostersUsedThisLevel.Count==0)
                return null; 
            // Create a copy of the list
            List<int> boostersUsedCopy = new List<int>(_boostersUsedThisLevel.ToList());

            Debug.Log("Boosters Used This Level: " + boostersUsedCopy.Count);
            // Clear the original list
            _boostersUsedThisLevel.Clear();

            // Return the copy
            return boostersUsedCopy;
        }
        private void AddBoostersUsed(int boosterId)
        {
            _boostersUsedThisLevel.Add(boosterId);
        }
        private void RemoveBoostersUsed(int boosterId)
        {
            _boostersUsedThisLevel.Remove(boosterId);
        }
        private void HandleMainEventGoalRemoved(IBoardItem item, int amount)
        {
            if (CollectedItems.ContainsKey(item.ItemID))
            {
                CollectedItems[item.ItemID] += amount;
            }
            else
            {
                CollectedItems.Add(item.ItemID, amount);
            }
        }
        
        

    }
}
