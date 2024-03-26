using Unity.Burst.Intrinsics;
using UnityEngine;

namespace _Scripts.Utility
{
    public static class ArrayExtensions
    {
        public static IItem GetItem(this IItem[,] array, Vector3Int position)
        {
            return array[position.x, position.y];
        }
        public static float GetDistance(this IItem[,] array,Vector2[,] cellPositions,int x, int y)
        {
            return Vector2.Distance(array[x,y].Transform.localPosition, cellPositions[x,y]);
        }
   
        public static int GetTypeID(this IItem[,] array, Vector3Int position)
        {
            return array[position.x, position.y].ItemType;
        }

        public static void Set(this IItem[,] array, Vector3Int position, IItem value)
        {
            array[position.x, position.y]= value;
        }
        
        public static Vector3 GetLocal(this Grid grid, Vector3Int position)
        {
            return grid.GetCellCenterWorld(new Vector3Int(position.x, position.y, position.z));
        }
        
        
    }
}