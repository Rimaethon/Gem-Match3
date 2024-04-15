using _Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Core
{
    public class ItemMovement
    {
        private readonly Board _board;
        private readonly int _width;
        private readonly int _height;
        private readonly bool[] _dirtyColumns;
        public ItemMovement(Board board, bool[] dirtyColumns, int width, int height)
        {
            _board = board;
            _width = width;
            _height = height;
            _dirtyColumns = dirtyColumns;
            
        }

        public void MoveItems()
        {
            for (int x = 0; x <_width; x++)
            {
               
                bool isAnyItemMoving=false;
                for (int y =0; y <_height ; y++)
                {
                    if (!_board.GetCell(x, y).HasItem || !_board.GetItem(x, y).IsActive ||
                       _board.GetCell(x,y).Item.IsSwapping)
                    {
                        isAnyItemMoving = true;
                        continue;
                    }
                    if (_board.GetItem(x,y).IsMoving)
                    {
                        MoveItemTowardsTarget(x, y);
                        isAnyItemMoving = true;
                        continue;
                    }
                    if(_board.GetCell(x, y).IsLocked)continue;

                   if(CanFallToAnyDirection(x, y))
                    {
                        
                        isAnyItemMoving = true;
                    }
                }

                if (isAnyItemMoving) continue;
                //_dirtyColumns[x] = false;
                for (int y = 0; y < _height; y++)
                {
                    if (!_board.GetCell(x, y).HasItem)
                        return;
                }
            }
        }
        
        private void MoveItemTowardsTarget(int x, int y)
        {
            _board.GetItem(x,y).SetSortingOrder(5);
            Vector2Int target = _board.GetItem(x,y).TargetToMove;
            Vector3 currPos = _board.GetItem(x,y).Transform.localPosition;
            Vector3 targetPos = LevelGrid.Instance.GetCellCenterLocalVector2(target);
            _board.GetItem(x,y).FallSpeed+=_board.GetItem(x,y).Gravity;
            float maxDistance = _board.GetItem(x,y).FallSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(currPos, targetPos, maxDistance);
            _board.GetItem(x,y).Transform.localPosition = newPos;
           
            if(IsItemCloseToTarget(newPos,targetPos,0.10f))
            {
                if ((target!=new Vector2Int(x,y))&&IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x)&&!_board.GetCell(target).HasItem)
                {
                    _board.GetCell(target).SetItem(_board.GetItem(x, y));
                    _board.GetCell(x, y).SetItem(null);
                    FinishItemMovement(target.x,target.y);
                }
                else
                {
                    FinishItemMovement(x, y);
                }
            }
            else if ((target!=new Vector2Int(x,y))&&IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x)&&!_board.GetCell(target).HasItem)
            { 
     //           Debug.Log(x+" "+y+" changed with "+target.x+" "+target.y+" and it was null "+_board.GetCell(target).HasItem);
                _board.GetCell(target).SetItem(_board.GetItem(x, y));
                _board.GetCell(target).SetIsGettingEmptied( false);
                _board.GetCell(x, y).SetItem(null);
            }

        }
        private void AddPositionToQueue(int x, int y,int targetX, int targetY)
        {
    //         Debug.Log("Adding position to queue "+x+" "+y+" to "+targetX+" "+targetY+" "+_board.GetCell(targetX,targetY).HasItem+" "+_board.GetCell(targetX,targetY).IsGettingFilled+" "+_board.GetCell(targetX,targetY).IsGettingEmptied);
            _board.GetItem(x,y).TargetToMove=new Vector2Int(targetX,targetY);
            _board.GetCell(targetX, targetY).SetIsGettingFilled( true);
            _board.GetItem(x,y).IsMoving = true;

            _board.GetCell(x,y).SetIsGettingEmptied( true);

        }
        private void RemovePositionFromQueue(int x, int y)
        {
            _board.GetCell(x,y).SetIsGettingEmptied( false);
            if (!_board.GetCell(x, y).HasItem)
                return;
            Vector2Int target=_board.GetItem(x,y).TargetToMove;
            _board.GetItem(x,y).Transform.localPosition = LevelGrid.Instance.GetCellCenterLocalVector2(target);
            _board.GetCell(target).SetIsGettingFilled(false);
        }
        private bool CanFallToAnyDirection(int x, int y)
        {
            if (!CheckIfFallAble(x, y ))
            {
                return false;
            }

            if (y > 0)
            { 
//             Debug.Log("Checking if item can fall"+x+" "+y+" to "+ (x,y-1)+" "+_board.GetCell(x,y-1).HasItem+" "+_board.GetCell(x,y-1).IsGettingFilled+" "+_board.GetCell(x,y-1).IsGettingEmptied+" "+_board.GetCell(x,y-1).IsLocked);

            }
            if (CanFallVertically(x, y))
            {
                AddPositionToQueue(x, y, x, y - 1);
                return true;
            }
            if(CanBelowFall(x,y))
                return false;
            if (x<_width-1&&CanFallDiagonally(x, y, x + 1))
            {
                AddPositionToQueue(x, y, x + 1, y - 1);                
                return true;
            }
            if (x>0&&CanFallDiagonally(x, y, x - 1))
            {
                AddPositionToQueue(x, y, x - 1, y - 1);    
                return true;
            }
            return false;
        }
        private bool CanBelowFall(int x, int y)
        {
            y--;
            while (y > 0)
            {
                if(!_board.GetCell(x,y).HasItem&&!_board.GetCell(x,y).IsLocked)
                    return true;
                if(_board.GetCell(x,y).IsGettingEmptied)
                    return true;
                y--;
            }
            return false;
        }
        private bool CanFallVertically(int x, int y)
        {
            if(_board.GetCell(x,y-1).IsLocked)
                return false;
            return (!_board.GetCell(x, y - 1).HasItem&&!_board.GetCell(x,y-1).IsGettingFilled)||(_board.GetCell(x, y - 1).HasItem&&_board.GetCell(x,y-1).IsGettingEmptied);
        }
        private bool CanFallDiagonally(int x, int y, int targetX)
        {
//           Debug.Log("Checking if item can fall"+x+" "+y+" to "+ targetX+" "+(y-1)+" "+_board.GetCell(targetX,y-1).HasItem+" "+_board.GetCell(targetX,y-1).IsGettingFilled+" "+_board.GetCell(targetX,y-1).IsGettingEmptied);
            if (_board.GetCell(targetX, y - 1).HasItem&&_board.GetCell(targetX,y-1).IsLocked) 
            {
         //   Debug.Log("Target is not empty"+_board.GetCell(targetX,y-1).Item.Position);
                 return false;
            }
            int firstObstacleAbove = GetFirstObstacleAbove(targetX, y);
            if (firstObstacleAbove == _height)
                return false;
            if(firstObstacleAbove==y&&(CheckIfFallAble(targetX,firstObstacleAbove)))
                 return false;
            if(firstObstacleAbove>y&&(CheckIfFallAble(targetX,firstObstacleAbove)||CheckIfFallAble(targetX+1,firstObstacleAbove)||CheckIfFallAble(targetX-1,firstObstacleAbove)))
                return false;
            if(firstObstacleAbove>y&&((_board.GetCell(x,y+1).HasItem&&_board.GetItem(x,y+1).IsFallAble)||_board.GetCell(x,y+1).IsGettingFilled))
                return false;
            return !_board.GetCell(targetX, y - 1).HasItem&&!_board.GetCell(targetX,y-1).IsGettingFilled;
        }
        private bool CheckIfFallAble(int x, int y)
        {
            if(x>=_width||x<0||y>=_height||y<1)
                return false;
            return _board.GetCell(x,y).HasItem&&_board.GetItem(x,y).IsFallAble&&!_board.GetCell(x,y).Item.IsSwapping||_board.GetCell(x,y).IsLocked;
        }
        private int GetFirstObstacleAbove(int x, int y)
        {
            while(y<_height)
            {
                if(_board.GetCell(x,y).HasItem&&!_board.GetCell(x,y).Item.IsFallAble)
                    return y;
                y++;
            }
            return y;
        }
        private bool IsItemCloseToTarget(Vector3 currPos, Vector3 targetPos, float maxDistance = 0.05f)
        {
            return Mathf.Abs(targetPos.y - currPos.y) < maxDistance/2 &&Mathf.Abs(targetPos.x - currPos.x) < maxDistance/2;
        }
        private void FinishItemMovement(int x, int y)
        { 
            _board.GetCell(x,y).SetIsGettingEmptied( false);
            RemovePositionFromQueue(x,y); 
            _board.GetItem(x, y).IsMoving=CanFallToAnyDirection(x, y);
            if (_board.GetItem(x, y).IsMoving) return;
            _board.GetItem(x,y).FallSpeed = 0f;
            _board.GetItem(x,y).SetSortingOrder(4);
            EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd,new Vector2Int(x,y));
         
        }
    }
}