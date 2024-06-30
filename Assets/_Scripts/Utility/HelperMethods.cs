using System.Collections.Generic;
using Scripts;
using UnityEngine;

namespace _Scripts.Utility
{
    public static class HelperMethods
    {
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

        public static bool IsInBoundaries(this Board board, Vector2Int pos)
        {

            return pos.x >= 0 && pos.x < board.Width&& pos.y >= 0 && pos.y < board.Height;
        }
        public static bool IsInBoundaries(this Board board, int x,int y)
        {
            return x >= 0 && x < board.Width&& y >= 0 && y < board.Height;
        }
        public static IBoardItem GetItem(this Board board, int x,int y)
        {
            if(board.Cells[x,y].HasItem)
            {
                return board.Cells[x,y].BoardItem;
            }

            return null;
        }
        public static IBoardItem GetItem(this Board board ,Vector2Int position)
        {
            if(board.IsInBoundaries(position)&&board.Cells[position.x,position.y].HasItem)
            {
                return board.Cells[position.x,position.y].BoardItem;
            }

            return null;
        }
        public static void SetBoardItemsParent(this Board board, Transform parent)
        {
            foreach (var cell in board.Cells)
            {
                if (cell.HasItem)
                {
                    cell.BoardItem.Transform.SetParent(parent);
                }
                if(cell.HasOverLayItem)
                {
                    cell.OverLayBoardItem.Transform.SetParent(parent);
                }
                if(cell.HasUnderLayItem)
                {
                    cell.UnderLayBoardItem.Transform.SetParent(parent);
                }
            }
        }
        #region Directions

        public static List<Vector2Int> GetFourDirections(this Vector2Int pos)
        {
            return new List<Vector2Int>()
            {
                new Vector2Int(pos.x, pos.y + 1), // Up
                new Vector2Int(pos.x, pos.y - 1), // Down
                new Vector2Int(pos.x + 1, pos.y), // Right
                new Vector2Int(pos.x - 1, pos.y)  // Left
            };
        }
        #endregion

    }
}
