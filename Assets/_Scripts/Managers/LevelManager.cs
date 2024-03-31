using System;
using System.Collections.Generic;
using _Scripts.Utility;
using JetBrains.Annotations;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.Managers
{
    public class LevelManager:Singleton<LevelManager>
    {
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> goalIds = new List<int>();
        private readonly Dictionary<int,List<Vector2Int>> _goals = new Dictionary<int, List<Vector2Int>>();
        private IEnumerable<ValueDropdownItem<int>> GetNormalItemIds()
        {
            foreach (var item in itemDatabase.NormalItems)
            {
                yield return new ValueDropdownItem<int>(item.Value.ItemPrefab.name, item.Key);
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.AddHandler<Board>(GameEvents.FindGoals, CheckGoal);
        }
        
        private void OnDisable()
        {
            if(EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler<Board>(GameEvents.FindGoals, CheckGoal);
        }


        private void CheckGoal(Board board)
        {
            foreach (var id in goalIds)
            {
                _goals.Add(id, new List<Vector2Int>());
            }

            for (int i = 0; i < board.Width; i++)
            {
                for (int j = 0; j < board.Height; j++)
                {

                    Cell cell = board.GetCell(i, j);
                    Vector2Int pos = new Vector2Int(i, j);
                    if (cell.HasItem && _goals.ContainsKey(cell.Item.ItemID))
                    {
                        _goals[cell.Item.ItemID].Add(pos);
                    }

                    if (cell.HasUnderLayItem && _goals.ContainsKey(cell.UnderLayItem.ItemID))
                    {
                        _goals[cell.UnderLayItem.ItemID].Add(pos);
                    }

                    if (cell.HasOverLayItem && _goals.ContainsKey(cell.OverLayItem.ItemID))
                    {
                        _goals[cell.OverLayItem.ItemID].Add(pos);

                    }
                    Debug.Log("Goal Check");
                }
            }
            foreach (var id in goalIds)
            {
                if (_goals[id].Count == 0)
                    _goals.Remove(id);
            }

            EventManager.Instance.Broadcast<Dictionary<int, List<Vector2Int>>>(GameEvents.OnGoalInitialization, _goals);
            Debug.Log("Goals Initialized");
        }
        
        public Vector2Int GetRandomGoalPos()
        {
            foreach (var goalID in  _goals.Keys)
            {
               if (_goals[goalID].Count > 0)
               { 
                   return _goals[goalID][UnityEngine.Random.Range(0, _goals[goalID].Count)];
               }
            }
            return new Vector2Int(-1, -1);
        }
    }
}