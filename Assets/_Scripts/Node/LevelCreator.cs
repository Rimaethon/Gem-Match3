using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.Data_Classes;
using _Scripts.Utility;
using DefaultNamespace;
using Scripts;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

[ExecuteInEditMode]
public class LevelCreator : MonoBehaviour
{
    [FoldoutGroup("Save Settıngs")]
    [SerializeField] private string tilePath = "Assets/Art/Tilemaps/Level Tiles/";
    [FoldoutGroup("Save Settıngs")]
    [SerializeField] private string levelDataPath = "Assets/Resources/Levels/";
    [FoldoutGroup("Save Settıngs")]
    [SerializeField] string levelDataName = "Level";
    [FoldoutGroup("Save Settıngs")]
    [SerializeField] string Extension = ".bytes";
    [FoldoutGroup("Save Settıngs")]
    [SerializeField] DataFormat dataFormat=DataFormat.Binary;
    [FoldoutGroup("Save Settıngs")]
    [SerializeField] private bool ifExistsDoNotGenerate;
    [FoldoutGroup("References")]
    [SerializeField] List<BoardDataCreator> boardDataCreators;
    [FoldoutGroup("References")]
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [ValueDropdown("GetNormalItemIds")]
    [SerializeField] private List<int> GoalIds = new List<int>();
    [SerializeField] private int levelID;
    [SerializeField] private int MoveCount;
    
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
        if (ifExistsDoNotGenerate && File.Exists(levelDataPath + levelDataName + levelID + Extension))
        {
            Debug.Log("Level Data Already Exists");
            return;
        }
        List<BoardData> _boards = GetBoardData();
        LevelData levelData = new LevelData
        {
            Boards = _boards,
            GoalSaveData = GetGoalData( _boards),
            MoveCount = MoveCount
        };
        string levelDataFinalPath = levelDataPath + levelDataName + levelID;
        SaveToJson(levelDataFinalPath, levelData);
    }
    private GoalSaveData GetGoalData(List<BoardData> boards)
    {
        int count = GoalIds.Count;
        int[] GoalIDs = GoalIds.ToArray();
        int[] GoalCounts = new int[count];
        foreach (var boardData in boards)
        {
            FindGoalsInBoard(boardData, GoalIDs, GoalCounts);

        }
        return new GoalSaveData(GoalIDs, GoalCounts);
    }
    private void FindGoalsInBoard(BoardData boardData, int[] GoalIDs, int[] GoalCounts)
    {
        for (int i = 0; i < boardData.NormalItemIds.GetLength(0); i++)
        {
            for (int j = 0; j < boardData.NormalItemIds.GetLength(1); j++)
            {
                int itemId = boardData.NormalItemIds[i, j];
                int index = Array.IndexOf(GoalIDs, itemId);
                if (index != -1)
                {
                    GoalCounts[index]++;
                }
                if(boardData.UnderlayItemIds.TryGetValue(new Vector2Int(i,j),out itemId))
                {
                    index = Array.IndexOf(GoalIDs, itemId);
                    if (index != -1)
                    {
                        GoalCounts[index]++;
                    }
                }

                if (boardData.OverlayItemIds.TryGetValue(new Vector2Int(i, j), out itemId))
                {
                    index = Array.IndexOf(GoalIDs, itemId);
                    if (index != -1)
                    {
                        GoalCounts[index]++;
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
            itemTileDataSO.sprite = itemData.ItemSprite;
            itemTileDataSO.gameObject = itemData.ItemPrefab;
            string subPath = "NormalTiles/";
            if (itemData.ItemPrefab.TryGetComponent(typeof(UnderlayItem), out _))
            {
                subPath = "UnderlayTiles/";
            }else if (itemData.ItemPrefab.TryGetComponent(typeof(OverlayItem), out _))
            {
                subPath = "OverlayTiles/";   
            }
            AssetDatabase.CreateAsset(itemTileDataSO, tilePath +subPath+ itemData.ItemPrefab.name + ".asset");
        }
    }

}