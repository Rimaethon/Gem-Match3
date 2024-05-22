using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Scripts.Data_Classes;
using _Scripts.Utility;
using DefaultNamespace;
using Scripts;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace _Scripts.Editor
{
    [ExecuteInEditMode]
    public class LevelCreator : MonoBehaviour
    {
        [FoldoutGroup("Save Settıngs")]
        [SerializeField] private string tilePath = "Assets/Art/Tilemaps/Level Tiles/";
        [FoldoutGroup("Save Settıngs")]
        [SerializeField] private string levelDataPath = "Assets/Data/Levels/";
        [FoldoutGroup("Save Settıngs")]
        [SerializeField] string Extension = ".json";
        [FoldoutGroup("Save Settıngs")]
        [SerializeField] DataFormat dataFormat=DataFormat.JSON;
        [FoldoutGroup("Save Settıngs")]
        [SerializeField] private bool ifExistsDoNotGenerate;
        [FoldoutGroup("References")]
        private List<BoardDataCreator> boardDataCreators;
        [FoldoutGroup("References")]
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> GoalIds = new List<int>();
        [SerializeField] private List<int> GoalCounts = new List<int>();
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> _spawnAbleFillerItemIds = new List<int>();
        [Tooltip("Write -1 if unlimited, this is specifically for spawning certain amount goals ")]
        [SerializeField] private List<int> _spawnAbleFillerItemCounts = new List<int>();
        [SerializeField] private int levelID;
        [SerializeField] private int MoveCount;
        [SerializeField] private int BackgroundID=0;
        
        private IEnumerable<ValueDropdownItem<int>> GetNormalItemIds()
        {
            foreach (var item in itemDatabase.NormalItems)
            {
                yield return new ValueDropdownItem<int>(item.Value.ItemPrefab.name, item.Key);
            }
        }
    
        [Button]
        public void CreateLevelData()
        {
            boardDataCreators = gameObject.GetComponentsInChildren<BoardDataCreator>().ToList();
            if(_spawnAbleFillerItemIds.Count!=_spawnAbleFillerItemCounts.Count)
            {
                Debug.LogError("Spawnable Filler Item Ids and Counts are not equal");
                return;
            }
            if(GoalIds.Count!=GoalCounts.Count)
            {
                Debug.LogError("Goal Ids and Counts are not equal");
                return;
            }
            if (ifExistsDoNotGenerate && File.Exists(levelDataPath + levelID + Extension))
            {
                Debug.Log("Level Data Already Exists");
                return;
            }
            List<BoardData> _boards = GetBoardData();
            LevelData levelData = new LevelData(_boards, _spawnAbleFillerItemIds, GetGoalData(_boards), BackgroundID, MoveCount);
            string levelDataFinalPath = levelDataPath + levelID;
            SaveToJson(levelDataFinalPath, levelData);
        }
        private GoalSaveData GetGoalData(List<BoardData> boards)
        {
            int[] goalIDs = GoalIds.ToArray();
            int[] goalCounts = GoalCounts.ToArray();
            return new GoalSaveData(goalIDs, goalCounts);
        }
        
        
        [Button]
        public void GetNumberOfGoalsInBoard()
        {
            boardDataCreators = gameObject.GetComponentsInChildren<BoardDataCreator>().ToList();
            GoalCounts = new List<int>(GoalIds.Count);
            foreach (var boardDataCreator in boardDataCreators)
            {
                
                BoardData boardData = boardDataCreator.CreateBoardData();
                for (int i = 0; i < itemDatabase.Boards[boardData.BoardSpriteID].Width; i++)
                {
                    for (int j = 0; j < boardData.NormalItemIds.GetLength(1); j++)
                    {
                        int itemId = boardData.NormalItemIds[i, j];
                        int index = GoalIds.IndexOf(itemId);
                        Debug.Log("Index: "+index);
                        if (index != -1)
                        {
                            GoalCounts[index]++;
                        }
                        if(boardData.UnderlayItemIds.TryGetValue(new Vector2Int(i,j),out itemId))
                        {
                            index =  GoalIds.IndexOf(itemId);
                            if (index != -1)
                            {
                                GoalCounts[index]++;
                            }
                        }

                        if (boardData.OverlayItemIds.TryGetValue(new Vector2Int(i, j), out itemId))
                        {
                            index =  GoalIds.IndexOf(itemId);
                            if (index != -1)
                            {
                                GoalCounts[index]++;
                            }
                        }

                    }
                }
            }

        }
        private List<BoardData> GetBoardData()
        {
            List<BoardData> boards = new List<BoardData>();
            foreach (var boardDataCreator in boardDataCreators)
            {
                boards.Add(boardDataCreator.CreateBoardData());
            }
            return boards;
        }
    
        private void SaveToJson(string path, LevelData levelData)
        {
            var serializedData = SerializationUtility.SerializeValue(levelData, dataFormat);
            File.WriteAllBytes(path+Extension, serializedData);
        }
        /// <summary>
        /// This method creates the Tile Scriptable Objects from the Item Database and saves them to the specified path
        /// </summary>
        [Button]
        public void InitializeTileScriptableObjectsFromItemDataBase()
        {
            if (itemDatabase == null)
            {
                Debug.LogError("Item Database is null");
                return;
            }
            //Normally I would make it delete old tiles but I believe it is too risky to do 
            foreach (var itemData in itemDatabase.NormalItems.Values)
            {
                var itemTileDataSO = ScriptableObject.CreateInstance<ItemTileDataSO>();
                itemTileDataSO.gameObject = itemData.ItemPrefab;
                itemTileDataSO.sprite = itemData.ItemPrefab.GetComponent<SpriteRenderer>().sprite;

                string subPath = "NormalTiles/";
                if (itemData.ItemPrefab.TryGetComponent(typeof(UnderlayBoardItem), out _))
                {
                    subPath = "UnderlayTiles/";
                }else if (itemData.ItemPrefab.TryGetComponent(typeof(OverlayBoardItem), out _))
                {
                    subPath = "OverlayTiles/";   
                }
                AssetDatabase.CreateAsset(itemTileDataSO, tilePath +subPath+ itemData.ItemPrefab.name + ".asset");
            }
        }

    }
}