using System.Collections.Generic;
using System.Linq;
using _Scripts.Utility;
using UnityEngine;

namespace _Scripts.Managers.Matching
{
    
    
    public static class MatchChecker
    {
        private static List<Vector2Int[]> _shapesRight = new List<Vector2Int[]>
        {
            new Vector2Int[] // Five in a row
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0),
                new Vector2Int(4, 0),
            },
            new Vector2Int[] // Five in a row
            {
                new Vector2Int(-2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
            },


            //since L is not symmetrical we need to add both sides of the L shape
            new Vector2Int[] // L shape upward tail 
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
            },
            new Vector2Int[] // L shape downward tail
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, -1),
                new Vector2Int(2, -2),
            },
            //since L is not symmetrical we need to add both sides of the L shape
            new Vector2Int[] // L shape upward tail 
            {
                new Vector2Int(-2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
            },
            new Vector2Int[] // L shape upward tail 
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
            },
            new Vector2Int[] // L shape downward tail
            {
                new Vector2Int(-2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(0, -1),
                new Vector2Int(0, -2),
            },
             new Vector2Int[] // T shape
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, -1)
            },
          
            new Vector2Int[] // T shape
            {
                new Vector2Int(0 ,0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            },
            new Vector2Int[] // Four in a row
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0),
            },
            new Vector2Int[] // Four in a row
            {
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
            },
            new Vector2Int[] // Four in a row
            {
                new Vector2Int(-2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
            },
            new Vector2Int[] // Square shape
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
            },
            new Vector2Int[] // Three in a row
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
            },
            new Vector2Int[] // Three in a row
            {
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
            },
            
        };
        
        private static List<Vector2Int[]> _allShapes = new List<Vector2Int[]>();

        private static bool _isRotated;
        public static List<Vector2Int> CheckMatches(this IItem[,] board,Vector2Int pos,int width, int height)
        {
            if (!_isRotated)
            {
                _allShapes= GenerateAllShapes(_shapesRight);
                _isRotated = true;
            }
            List<Vector2Int> matchedItems = new List<Vector2Int>();
            
            foreach (var shape in _allShapes)
            {
                

               matchedItems= CheckShape(board,shape, pos,width, height);

               if (matchedItems.Count == shape.Length)
               {
                   return matchedItems;
               }
               matchedItems.Clear();

               
            }
            return matchedItems;
            
            
        }
        //Actually instead of creating a list it would be better to use a array that is created in the begining but I need to create a structure that handles race conditions that can occur.   
        private static List<Vector2Int> CheckShape(IItem[,] board, Vector2Int[] shape, Vector2Int pos, int width, int height)
        {
            List<Vector2Int> matchedItems = new List<Vector2Int>();
            if(board.GetItem(pos)==null) return matchedItems;
            int shapeSize = shape.Length;

         
            // Iterate over the shape's pattern
            for (int i = 0; i < shapeSize; i++)
            {
                int x = pos.x + shape[i].x;
                int y = pos.y + shape[i].y;
                if(x>=width || x<0 || y>=height || y<0) return matchedItems;
                
               if(board[x,y]==null||(board[x,y].MovementQueue.Count>0&&!(x==pos.x&&y==pos.y))) return new List<Vector2Int>();
                if (board[x, y].ItemType != board.GetTypeID(pos)) return new List<Vector2Int>();

                matchedItems.Add(new Vector2Int(x,y));
            }

            // If we've made it this far, the shape matches
            return matchedItems;
        }
  
        private static List<Vector2Int[]> GenerateAllShapes(List<Vector2Int[]> shapes)
        {
            List<Vector2Int[]> allShapes = new List<Vector2Int[]>();



            foreach (var shape in shapes)
            {
                Vector2Int[] currentShape = shape;

                // Add the original shape and its reflection
                allShapes.Add(currentShape);
                allShapes.Add(ReflectShape(currentShape));

                // Generate all rotations and reflections
                for (int i = 0; i < 3; i++)
                {
                    currentShape = RotateShape(currentShape);
                    allShapes.Add(currentShape);
                    allShapes.Add(ReflectShape(currentShape));
                }
            }

            return allShapes;

        }

        
        private static Vector2Int[] RotateShape(Vector2Int[] shape)
        {
            Vector2Int[] rotatedShape = new Vector2Int[shape.Length];
            for (int i = 0; i < shape.Length; i++)
            {
                // Swap x and y, then negate the new y
                rotatedShape[i] = new Vector2Int(shape[i].y, -shape[i].x);
            }
            return rotatedShape;
        }

        private static Vector2Int[] ReflectShape(Vector2Int[] shape)
        {
            Vector2Int[] reflectedShape = new Vector2Int[shape.Length];
            for (int i = 0; i < shape.Length; i++)
            {
                // Negate x
                reflectedShape[i] = new Vector2Int(-shape[i].x, shape[i].y);
            }
            return reflectedShape;
        }

    
    }
}