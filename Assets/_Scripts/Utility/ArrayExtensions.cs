using System.Collections.Generic;
using Scripts;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace _Scripts.Utility
{
    public static class ArrayExtensions
    {
        
        public static Cell GetCell(this Board board, int x,int y)
        {
            return board.Cells[x, y];
        }
        public static Cell GetCell(this Board board, Vector2Int position)
        {
            
            return board.Cells[position.x, position.y];
        }
        public static Cell GetCell(this Cell[,] array, int x,int y)
        {
            return array[x, y];
        }
        public static Cell GetCell(this Cell[,] array, Vector2Int position)
        {
            return array[position.x, position.y];
        }
        
        public static float GetBoardBoundaryLeftX(this Cell[,] array)
        {
            return LevelGrid.Instance.GetCellCenterWorld(array[0,0].CellPosition).x-LevelGrid.Grid.cellSize.x;
        }
        public static float GetBoardBoundaryRightX(this Cell[,] array)
        {
            return LevelGrid.Instance.GetCellCenterWorld(array[array.GetLength(0)-1,0].CellPosition).x+LevelGrid.Grid.cellSize.x;
        }
        public static float GetBoardBoundaryTopY(this Cell[,] array)
        {
            return LevelGrid.Instance.GetCellCenterWorld(array[0,array.GetLength(1)-1].CellPosition).y+LevelGrid.Grid.cellSize.y;
        }
        public static float GetBoardBoundaryBottomY(this Cell[,] array)
        {
            return LevelGrid.Instance.GetCellCenterWorld(array[0,0].CellPosition).y-LevelGrid.Grid.cellSize.y;
        }
        public static IItem GetItem(this Cell[,] array, int x,int y)
        {
            if(array.GetCell(x,y).HasItem)
            {
                return array.GetCell(x,y).Item;
            }
          
            return null;
        }
        public static IItem GetItem(this Cell[,] array, Vector2Int position)
        {
            if(array.IsInBoundaries(position)&&array.GetCell(position).HasItem)
            {
                return array.GetCell(position).Item;
            }

            return null;
        }
        public static bool IsInBoundaries(this Cell[,] array, Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < array.GetLength(0)&& pos.y >= 0 && pos.y < array.GetLength(1);
        }
        public static IItem GetItem(this Board board, int x,int y)
        {
            if(board.Cells.GetCell(x,y).HasItem)
            {
                return board.Cells.GetCell(x,y).Item;
            }
          
            return null;
        }
        public static IItem GetItem(this Board board ,Vector2Int position)
        {
            if(IsInBoundaries(board.Cells,position)&&board.Cells.GetCell(position).HasItem)
            {
                return board.Cells.GetCell(position).Item;
            }

            return null;
        }

        

    }
}