using System.Collections.Generic;
using _Scripts.Data_Classes;
using _Scripts.Utility;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Scripts;
using Unity.Mathematics;
using UnityEngine;
namespace _Scripts.Core
{
    public class ItemMovement
    {
        private readonly Board _board;
        private bool[] _dirtyColumns;
        private const float Gravity = 0.5f;

        public ItemMovement(Board board, bool[] dirtyColumns)
        {
            _board = board;
            _dirtyColumns = dirtyColumns;
            EventManager.Instance.AddHandler(GameEvents.OnShuffleBoard, ShuffleItems);
        }
        public void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnShuffleBoard, ShuffleItems);

        }

        public bool MoveItems()
        {
            bool isAnyItemMoving = false;
            for (int x = 0; x < _board.Width; x++)
            {
                bool isAnyItemMovingInColumn = false;
                bool hasObstacle = false;
                if(!_dirtyColumns[x])
                    continue;
                for (int y = _board.Height-1; y>=0; y--)
                {
                    if (!_board.Cells[x, y].HasItem)
                    {
                        isAnyItemMovingInColumn = true;
                        continue;
                    }
                    if (_board.Cells[x, y].BoardItem.IsSwapping || _board.GetItem(x, y).IsExploding ||
                        _board.GetItem(x, y).IsMatching)
                    {
                        isAnyItemMovingInColumn = true;
                        continue;
                    }
                    if (!_board.GetItem(x, y).IsFallAble&&!_board.GetItem(x,y).IsGeneratorItem)
                    {
                        hasObstacle = true;
                        continue;
                    }
                    if (_board.GetItem(x, y).IsMoving)
                    {
                        MoveItemTowardsTarget(x, y);
                        isAnyItemMovingInColumn = true;
                        continue;
                    }
                    if (_board.Cells[x, y].IsLocked)
                    {
                        continue;
                    }
                    if (CanFallToAnyDirection(x, y))
                    {
                        isAnyItemMovingInColumn = true;
                    }
                }

                if (isAnyItemMovingInColumn)
                {
                    isAnyItemMoving = true;
                    continue;
                }
                if (hasObstacle)
                {
                    if(x>0)
                        _dirtyColumns[x-1]=true;
                    if(x<_board.Width-1)
                        _dirtyColumns[x+1]=true;
                }
                _dirtyColumns[x] = false;
            }

            return isAnyItemMoving;
        }

        private void MoveItemTowardsTarget(int x, int y)
        {
            Vector2Int target = _board.GetItem(x, y).TargetToMove;
            Vector3 currPos = _board.GetItem(x, y).Transform.localPosition;
            Vector3 targetPos = LevelGrid.Instance.GetCellCenterLocalVector2(target);
            _board.GetItem(x, y).FallSpeed += Gravity;
            if (y > 0 && _board.Cells[x, y-1].HasItem && _board.GetItem(x, y - 1).IsMoving)
                _board.GetItem(x, y).FallSpeed =
                    math.min(_board.GetItem(x, y).FallSpeed, _board.GetItem(x, y - 1).FallSpeed);
            float maxDistance = _board.GetItem(x, y).FallSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(currPos, targetPos, maxDistance);
            _board.GetItem(x, y).Transform.localPosition = newPos;

            if (IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x / 10))
            {
                if ((target != new Vector2Int(x, y)) &&
                    IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x) &&
                    !_board.Cells[target.x, target.y].HasItem)
                {
                    _board.Cells[target.x, target.y].SetItem(_board.GetItem(x, y));
                    _board.Cells[x, y].SetItem(null);

                    FinishItemMovement(target.x, target.y);
                }
                else
                {

                    FinishItemMovement(x, y);
                }
            }
            else if ((target != new Vector2Int(x, y)) &&
                     IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x+(LevelGrid.Grid.cellSize.x*2*math.abs(target.y-y-1))) &&
                     !_board.Cells[target.x, target.y].HasItem)
            {
                _board.Cells[target.x, target.y].SetItem(_board.GetItem(x, y));
                _board.Cells[x, y].SetIsGettingEmptied(false);
                _board.Cells[target.x, target.y].SetIsGettingFilled(false);
                _board.Cells[x, y].SetItem(null);
            }

        }

        private void GiveTargetToItem(int x, int y, int targetX, int targetY)
        {
            _board.GetItem(x, y).TargetToMove = new Vector2Int(targetX, targetY);
            _board.Cells[targetX, targetY].SetIsGettingFilled(true);
            _board.GetItem(x, y).IsMoving = true;

            _board.Cells[x, y].SetIsGettingEmptied(true);

        }

        private void RemovePositionFromQueue(int x, int y)
        {
            _board.Cells[x, y].SetIsGettingEmptied(false);
            if (!_board.Cells[x, y].HasItem)
                return;
            Vector2Int target = _board.GetItem(x, y).TargetToMove;
            _board.GetItem(x, y).Transform.localPosition = LevelGrid.Instance.GetCellCenterLocalVector2(target);
            _board.Cells[target.x,target.y].SetIsGettingFilled(false);
        }

        private bool CanFallToAnyDirection(int x, int y)
        {
            if (!CheckIfFallAble(x, y))
            {
                return false;
            }

            if (_board.Cells[x, y - 1].CellType == CellType.SHIFTER)
            {
                int posToFall=HasAnyEmptyCellAfterShifter(x, y);

                if (posToFall != -1)
                {
                    GiveTargetToItem(x, y, x, posToFall);
                    return true;
                }
                return false;
            }
            if (CanFallBelow(x, y))
            {
                GiveTargetToItem(x, y, x, y - 1);

                return true;
            }


            if (IsAnyItemBelowFallingOrEmpty(x, y))
            {
                return false;
            }

            if (x < _board.Width - 1 && CanFallDiagonally(x, y, x + 1))
            {
                GiveTargetToItem(x, y, x + 1, y - 1);
                return true;
            }

            if (x >0 && CanFallDiagonally(x, y, x - 1))
            {
                GiveTargetToItem(x, y, x - 1, y - 1);
                return true;
            }

            return false;
        }

        private bool IsAnyItemBelowFallingOrEmpty(int x, int y)
        {
            y--;
            while (y > 0)
            {
                if (!_board.Cells[x,y].HasItem && !_board.Cells[x,y].IsLocked)
                    return true;
                if (_board.Cells[x,y].IsGettingEmptied)
                    return true;
                if (_board.Cells[x,y].HasItem && !_board.GetItem(x, y).IsFallAble)
                    return false;
                y--;
            }
            return false;
        }
        private int HasAnyEmptyCellAfterShifter(int x, int y)
        {
            y--;
            while (y > 0&&_board.Cells[x,y].CellType==CellType.SHIFTER)
            {
                y--;
            }
            if(!CanFallBelow(x,y+1))
                return -1;

            while (y>0 &&CanFallBelow(x,y))
            {
                y--;
            }
            return y;
        }

        private bool CanFallBelow(int x, int y)
        {
            if (_board.Cells[x,y-1].IsLocked)
                return false;

            return (!_board.Cells[x,y-1].HasItem && !_board.Cells[x,y-1].IsGettingFilled) ||
                   (_board.Cells[x,y-1].HasItem && _board.Cells[x,y-1].IsGettingEmptied);
        }

        private bool CheckIfFallAble(int x, int y)
        {
            if (!_board.IsInBoundaries(x, y - 1)||_board.Cells[x,y-1].CellType==CellType.BLANK)
                return false;
            return _board.Cells[x,y].HasItem && _board.GetItem(x, y).IsFallAble &&
                !_board.Cells[x,y].BoardItem.IsSwapping || _board.Cells[x,y].IsLocked;
        }

        private bool CanFallDiagonally(int x, int y, int targetX)
        {
            if (_board.Cells[targetX, y - 1].HasItem || _board.Cells[targetX, y - 1].IsLocked)
            {
                return false;
            }

            int firstObstacleAbove = GetFirstObstacleAbove(targetX, y);
            if (firstObstacleAbove == _board.Height)
            {
                return false;
            }

            if (firstObstacleAbove == y && CheckIfFallAble(targetX, firstObstacleAbove))
            {
                return false;
            }

            if (firstObstacleAbove > y && (CheckIfFallAble(targetX, firstObstacleAbove) ||
                                           CheckIfFallAble(targetX + 1, firstObstacleAbove) ||
                                           CheckIfFallAble(targetX - 1, firstObstacleAbove)))
            {
                return false;
            }

            if (firstObstacleAbove > y && ((_board.Cells[x, y+1].HasItem && _board.GetItem(x, y + 1).IsFallAble) ||
                                           _board.Cells[x, y+1].IsGettingFilled))
            {
                return false;
            }

            bool canFall = !_board.Cells[targetX, y-1].HasItem && !_board.Cells[targetX, y-1].IsGettingFilled;
            return canFall;
        }

        private int GetFirstObstacleAbove(int x, int y)
        {
            while (y < _board.Height)
            {
                if (_board.Cells[x, y].HasItem && !_board.Cells[x, y].BoardItem.IsFallAble)
                    return y;
                y++;
            }

            return y;
        }

        private bool IsItemCloseToTarget(Vector3 currPos, Vector3 targetPos, float maxDistance = 0.05f)
        {
            return Mathf.Abs(targetPos.y - currPos.y) < maxDistance / 2 &&
                   Mathf.Abs(targetPos.x - currPos.x) < maxDistance / 2;
        }

        private void FinishItemMovement(int x, int y)
        {
            _board.Cells[x, y].SetIsGettingEmptied(false);
            RemovePositionFromQueue(x, y);
            _board.GetItem(x, y).IsMoving = CanFallToAnyDirection(x, y);
            if (_board.GetItem(x, y).IsMoving) return;
            _board.GetItem(x, y).FallSpeed = 1.5f;
            if(y<_board.Height-1&&_board.Cells[x,y+1].HasItem&&_board.GetItem(x,y+1).IsMoving&&_board.GetItem(x,y+1).ItemID==_board.GetItem(x,y).ItemID)
                return;
            EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd, new Vector2Int(x, y));

        }

        #region Jester Hat Shuffle
            private void ShuffleItems()
            {
                List<Vector2Int> shuffleableCells = new List<Vector2Int>();
                for (int x = 0; x < _board.Width; x++)
                {
                    for (int y = 0; y < _board.Height; y++)
                    {
                        if (_board.Cells[x, y].HasItem && _board.GetItem(x, y).IsShuffleAble)
                        {
                            shuffleableCells.Add(new Vector2Int(x, y));
                        }
                    }
                }
                // Ensure the number of shuffleable cells is even
                if (shuffleableCells.Count % 2 != 0)
                {
                    shuffleableCells.RemoveAt(shuffleableCells.Count - 1);
                }
                System.Random rand = new System.Random();
                while (shuffleableCells.Count > 0)
                {
                    int index1 = rand.Next(shuffleableCells.Count);
                    int index2 = rand.Next(shuffleableCells.Count);
                    if(index1==index2)
                        continue;
                    SwapItems(shuffleableCells[index1], shuffleableCells[index2]);
                    if (index1 > index2)
                    {
                        shuffleableCells.RemoveAt(index1);
                        shuffleableCells.RemoveAt(index2);
                    }
                    else
                    {
                        shuffleableCells.RemoveAt(index2);
                        shuffleableCells.RemoveAt(index1);
                    }
                }

            }
            private void SwapItems(Vector2Int pos1, Vector2Int pos2)
            {
                IBoardItem item1 = _board.GetItem(pos1);
                IBoardItem item2 = _board.GetItem(pos2);
                Vector3 item1Pos = item1.Transform.localPosition;
                Vector3 item2Pos = item2.Transform.localPosition;
                item1.Transform.DOLocalMove(item2Pos, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
                item2.Transform.DOLocalMove(item1Pos, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
                    () =>
                    {
                      IBoardItem temp = _board.GetItem(pos1);
                        _board.Cells[pos1.x, pos1.y].SetItem(_board.GetItem(pos2));
                        _board.Cells[pos2.x, pos2.y].SetItem(temp);
                        EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd, pos1);
                        EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd, pos2);
                    };
            }
        #endregion

    }
}
