using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Data_Classes
{
    public class BoardSpriteSaveData
    {
        public int Width;
        public int Height;
        public HashSet<Vector2Int> BlankCells;
        public Sprite Sprite;
        public BoardSpriteSaveData(Sprite sprite, int width, int height, HashSet<Vector2Int> blankCells)
        {
            Sprite = sprite;
            Width = width;
            Height = height;
            BlankCells = blankCells;
        }
        
    }
}