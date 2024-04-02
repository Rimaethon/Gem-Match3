using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using UnityEngine;

namespace Scripts
{
    [Serializable]
    public class Board
    {
        public Cell[,] Cells;
        public int Width => _width;
        public int Height=> _height;
        private int _width;
        private int _height;
        public Board(int width, int height)
        {
            _width= width;
            _height = height;
            Cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cells[x, y] = new Cell(new Vector2Int(x,y));
                }
            }
  
        }
       
    }
}