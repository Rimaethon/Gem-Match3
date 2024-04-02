using System;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace Scripts.Node
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private bool ifExistsDonotGenerate;
        [SerializeField] private string path = "Assets/Resources/Levels/Level ";
        [SerializeField] private int levelID = 1;
        [SerializeField] private int backgroundID;
        [ShowInInspector] [TableMatrix(DrawElementMethod = "DrawElement", RowHeight = 90)]
        [SerializeField] private int[,] goalIds=new int[4,1];
        [SerializeField] private int moveCount = 1;
        [SerializeField] private ItemDatabaseSO itemDataBase;
        private readonly LevelData _levelData = new();
        private readonly List<int> _goalCounts = new();
        [ShowInInspector] [TableMatrix(DrawElementMethod = "DrawElement", RowHeight = 90)]
        public int[,] BoardElementIDs = new int[8, 10];

        private int DrawGoals(Rect rect, int value)
        {
            if (itemDataBase.NormalItems.Count == 0)
            {
                Debug.LogError("Board Elements are empty");
                return value;
            }

            var val = itemDataBase.NormalItems.Count;
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = (value + 1) % val;
                GUI.changed = true;
                Event.current.Use();
            }

            // Increase the size of the rectangle
            // Draw the texture with a larger scale
            EditorGUI.DrawPreviewTexture(rect, SpriteUtility.GetSpriteTexture
                    (itemDataBase.NormalItems[value].ItemPrefab.GetComponent<SpriteRenderer>().sprite, false),
                null, ScaleMode.ScaleToFit);
            return value;
        }

        private void Reset()
        {
            for (int i = 0; i < 4; i++)
            {
                goalIds[i, 0] = -1;
            }
        }

        private int DrawElement(Rect rect, int value)
        {
            
            if (itemDataBase.NormalItems.Count == 0)
            {
                Debug.LogError("Board Elements are empty");
                return value;
            }

            var val = itemDataBase.NormalItems.Count+1;
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = ((value + 1) % val)-1;
                GUI.changed = true;
                Event.current.Use();
            }

            if (value != -1)
            {
                EditorGUI.DrawPreviewTexture(rect, SpriteUtility.GetSpriteTexture
                        (itemDataBase.NormalItems[value].ItemPrefab.GetComponent<SpriteRenderer>().sprite, false),
                    null, ScaleMode.ScaleToFit);
            }
           
            return value;
        }


        [Button]
        private void GenerateNonRectangularGrid()
        {
            if (ifExistsDonotGenerate && File.Exists(path + levelID + ".json"))
            {
                Debug.LogError($"Level {levelID} already exists");
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

        }

       
    }
}