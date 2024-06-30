using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Data_Classes
{
    public class BoardSpriteSaveData
    {
        public int Width = 8;
        public int Height = 10;
        [TableMatrix(DrawElementMethod = "DrawCell", ResizableColumns = true)]
        public CellType[,] CellTypeMatrix;
        public Sprite Sprite;

        [Button]
        public void ResetCells()
        {
            CellTypeMatrix = new CellType[Width, Height];
        }

        private CellType DrawCell(Rect rect, CellType value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if ( CellType.NORMAL == value)
                {
                    value = CellType.SPAWNER;
                }
                else if ( CellType.SPAWNER == value)
                {
                    value = CellType.BLANK;
                }
                else if (CellType.BLANK == value)
                {
                    value = CellType.SHIFTER;
                }
                else if(CellType.SHIFTER==value)
                    value = (int)CellType.NORMAL;
                GUI.changed = true;
                Event.current.Use();
            }
            EditorGUI.DrawRect(rect, value == CellType.SPAWNER   ? Color.green
                                     : value ==CellType.BLANK   ? Color.red
                                     : value == CellType.SHIFTER ? Color.yellow
                                                                        : Color.gray);

            return value;
        }

    }

    public enum CellType
    {
        NORMAL=0,
        SPAWNER=1,
        BLANK=2,
        SHIFTER=3
    }
}
