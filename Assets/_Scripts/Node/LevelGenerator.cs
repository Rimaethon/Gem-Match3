using System;
using System.Collections.Generic;
using DefaultNamespace;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

namespace Scripts.Node
{
    public class LevelGenerator : MonoBehaviour
    {
        public bool IfExistsDonotGenerate;
        public String path="Assets/Resources/Levels/Level ";
        public int levelID=1;
        public int backgroundID=0;
        
        
        [HorizontalGroup]
        public List<int> GoalSpriteIds;
        [HorizontalGroup]
        public List<int> GoalCounts;
       
        public int moveCount=1;
        [SerializeField] public ItemDatabaseSO boardElements;
        
        [ShowInInspector][TableMatrix(DrawElementMethod = "DrawElement",RowHeight = 90)]
        public int[,] BoardElementIDs=new int[8,10];
        private LevelData _levelData= new LevelData();
       
        
        
        private int DrawElement(Rect rect, int value)
        {
            if(boardElements.BoardElements.Count==0)
            {
                Debug.LogError("Board Elements are empty");
                return value;
            }

            int val = boardElements.BoardElements.Count;
            if(Event.current.type==EventType.MouseDown&&rect.Contains(Event.current.mousePosition))
            {
                value = (value + 1) % val;
                GUI.changed = true;
                Event.current.Use();
            }

            // Increase the size of the rectangle
            
            // Draw the texture with a larger scale
            EditorGUI.DrawPreviewTexture(rect, SpriteUtility.GetSpriteTexture
                (boardElements.BoardElements[value].ItemPrefab.GetComponent<SpriteRenderer>().sprite,false),
                null, ScaleMode.ScaleToFit);
            return value;
        }
        
        
        
        
        
        
        [Button]
        private void GenerateNonRectangularGrid()
        {
            if (IfExistsDonotGenerate && System.IO.File.Exists(path + levelID + ".json"))
            {
                Debug.LogError($"Level {levelID} already exists");
                return;
            }
            if(GoalCounts.Count!=GoalSpriteIds.Count || GoalSpriteIds.Count>4)
            {
                Debug.LogError("Goal Sprite IDs and Goal Counts must have the same length and must be less than 5");
                return;
            }

            if (moveCount <= 0)
            {
                Debug.LogError("Move Count must be greater than 0");
                return;
            }

            if (BoardElementIDs.GetLength(0) == 0 || BoardElementIDs.GetLength(1) == 0)
            {
                Debug.LogError("Board Element IDs must have at least 1 row and 1 column");
                return;
            }
            
            _levelData.LevelID = levelID;
            _levelData.BackgroundID = backgroundID;
            _levelData.GoalSpriteIDs = GoalSpriteIds.ToArray();
            _levelData.GoalCounts = GoalCounts.ToArray();
            _levelData.MoveCount = moveCount;
            _levelData.BoardElementIDs = BoardElementIDs;            
            SaveToJson();
        }
        
        private void SaveToJson()
        {
            byte[] serializedData = Sirenix.Serialization.SerializationUtility.SerializeValue(_levelData, Sirenix.Serialization.DataFormat.JSON);
            System.IO.File.WriteAllBytes(path + levelID + ".json", serializedData);
        }
    }
}