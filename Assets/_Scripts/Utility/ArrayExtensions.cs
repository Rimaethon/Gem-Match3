using Unity.Burst.Intrinsics;
using UnityEngine;

namespace _Scripts.Utility
{
    public static class ArrayExtensions
    {
        public static IItem GetItem(this IItem[,] array, Vector2Int position)
        {
            return array[position.x, position.y];
        }
        public static float GetDistance(this IItem[,] array,Vector2[,] cellPositions,int x, int y)
        {
            return Vector2.Distance(array[x,y].Transform.localPosition, cellPositions[x,y]);
        }
   
        public static int GetTypeID(this IItem[,] array, Vector2Int position)
        {
            return array[position.x, position.y].ItemType;
        }

        public static void Set(this IItem[,] array, Vector3Int position, IItem value)
        {
            array[position.x, position.y]= value;
        }
        
        public static Vector2 GetCellCenterLocalVector2(this Grid grid, Vector2Int position)
        {
            return new Vector2(grid.GetCellCenterLocal(new Vector3Int(position.x,position.y,0)).x,grid.GetCellCenterLocal(new Vector3Int(position.x,position.y,0)).y);
        }
        public static Vector2 GetCellCenterWorldVector2(this Grid grid, Vector2Int position)
        {
            return new Vector2(grid.GetCellCenterWorld(new Vector3Int(position.x,position.y,0)).x,grid.GetCellCenterWorld(new Vector3Int(position.x,position.y,0)).y);
        }


        public static Vector2Int WorldToCellVector2Int(this Grid grid, Vector2 pos)
        {
            return new Vector2Int(grid.WorldToCell(pos).x, grid.WorldToCell(pos).y);
        }
        
    }
}