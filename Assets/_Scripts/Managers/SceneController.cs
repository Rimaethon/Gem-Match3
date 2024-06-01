using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

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

        protected override void Awake()
        {
            base.Awake();
            SceneManager.LoadScene(0);
            
            
        }
        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneLoaded;

            EventManager.Instance.AddHandler(GameEvents.OnLevelButtonPressed, SwitchToGameScene);
            EventManager.Instance.AddHandler<int>(GameEvents.OnBoosterUsed, AddBoostersUsed);
            EventManager.Instance.AddHandler<int>(GameEvents.OnBoosterRemoved, RemoveBoostersUsed);
            EventManager.Instance.AddHandler<int,int>(GameEvents.OnMainEventGoalRemoval, HandleMainEventGoalRemoved);
            EventManager.Instance.AddHandler(GameEvents.OnLevelRestart,HandleRestart);
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
            EventManager.Instance.AddHandler(GameEvents.OnLevelCompleted, () =>
            {
                IsLevelCompleted = true;
            });
        }

        private void OnSceneLoaded(Scene arg0)
        {
            GC.Collect();
        }
    
        private void HandleRestart()
        {
            CollectedItems.Clear();
            IsLevelCompleted = false;
            SceneManager.LoadScene(1);
        }


        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneLoaded;

            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnLevelFailed, () => { SceneManager.LoadScene(0); });
            EventManager.Instance.RemoveHandler<int>(GameEvents.OnBoosterUsed, AddBoostersUsed);
            EventManager.Instance.RemoveHandler<int>(GameEvents.OnBoosterRemoved, RemoveBoostersUsed);
            EventManager.Instance.RemoveHandler(GameEvents.OnLevelButtonPressed, SwitchToGameScene);
            EventManager.Instance.RemoveHandler<int,int>(GameEvents.OnMainEventGoalRemoval, HandleMainEventGoalRemoved);
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
        private void HandleMainEventGoalRemoved(int itemID, int amount)
        {
            if (CollectedItems.ContainsKey(itemID))
            {
                CollectedItems[itemID] += amount;
            }
            else
            {
                CollectedItems.Add(itemID, amount);
            }
        }
        
        

    }
}
