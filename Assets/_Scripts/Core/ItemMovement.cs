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
        private readonly Board board;
        private readonly bool[] dirtyColumns;
        private const float Gravity = 0.5f;

        public ItemMovement(Board board, bool[] dirtyColumns)
        {
            this.board = board;
            this.dirtyColumns = dirtyColumns;
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
            for (int x = 0; x < board.Width; x++)
            {
                bool isAnyItemMovingInColumn = false;
                bool hasObstacle = false;
                if(!dirtyColumns[x])
                    continue;
                for (int y = board.Height-1; y>=0; y--)
                {

                    if (board.Cells[x,y].CellType==CellType.SHIFTER
                                                   ||board.Cells[x,y].CellType==CellType.BLANK)
                    {
                        continue;
                    }
                    if (board.Cells[x, y].IsGettingFilled || board.Cells[x, y].IsGettingEmptied||board.Cells[x,y].IsLocked)
                    {
                        isAnyItemMovingInColumn = true;
                    }
                    if (!board.Cells[x, y].HasItem)
                    {
                        continue;
                    }
                    if (board.Cells[x, y].BoardItem.IsSwapping || board.GetItem(x, y).IsExploding ||
                        board.GetItem(x, y).IsMatching)
                    {
                        isAnyItemMovingInColumn = true;
                        continue;
                    }
                    if (!board.GetItem(x, y).IsFallAble&&!board.GetItem(x,y).IsGeneratorItem)
                    {
                        hasObstacle = true;
                        continue;
                    }
                    if (board.GetItem(x, y).IsMoving)
                    {
                        MoveItemTowardsTarget(x, y);
                        isAnyItemMovingInColumn = true;
                        continue;
                    }
                    if (board.Cells[x, y].IsLocked)
                    {
                        continue;
                    }
                    if (CanFallToAnyDirection(x, y))
                    {
                        isAnyItemMovingInColumn = true;
                    }
                }
                if (hasObstacle)
                {
                    if(x>0)
                        dirtyColumns[x-1]=true;
                    if(x<board.Width-1)
                        dirtyColumns[x+1]=true;
                }
                if (isAnyItemMovingInColumn)
                {
                    isAnyItemMoving = true;
                    continue;
                }
                dirtyColumns[x] = false;
            }

            return isAnyItemMoving;
        }

        private void MoveItemTowardsTarget(int x, int y)
        {
            Vector2Int target = board.GetItem(x, y).TargetToMove;
            Vector3 currPos = board.GetItem(x, y).Transform.localPosition;
            Vector3 targetPos = LevelGrid.Instance.GetCellCenterLocalVector2(target);
            board.GetItem(x, y).FallSpeed += Gravity;
            if (y > 0 && board.Cells[x, y-1].HasItem && board.GetItem(x, y - 1).IsMoving)
                board.GetItem(x, y).FallSpeed =
                    math.min(board.GetItem(x, y).FallSpeed, board.GetItem(x, y - 1).FallSpeed);
            float maxDistance = board.GetItem(x, y).FallSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(currPos, targetPos, maxDistance);
            board.GetItem(x, y).Transform.localPosition = newPos;

            if (IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x / 10))
            {
                if ((target != new Vector2Int(x, y)) &&
                    IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x) &&
                    !board.Cells[target.x, target.y].HasItem)
                {
                    board.Cells[target.x, target.y].SetItem(board.GetItem(x, y));
                    board.Cells[x, y].SetItem(null);

                    FinishItemMovement(target.x, target.y);
                }
                else
                {

                    FinishItemMovement(x, y);
                }
            }
            else if ((target != new Vector2Int(x, y)) &&
                     IsItemCloseToTarget(newPos, targetPos, LevelGrid.Grid.cellSize.x+(LevelGrid.Grid.cellSize.x*2*math.abs(target.y-y-1))) &&
                     !board.Cells[target.x, target.y].HasItem)
            {
                board.Cells[target.x, target.y].SetItem(board.GetItem(x, y));
                board.Cells[x, y].SetIsGettingEmptied(false);
                board.Cells[target.x, target.y].SetIsGettingFilled(false);
                dirtyColumns[target.x] = true;
                board.Cells[x, y].SetItem(null);
            }

        }

        private void GiveTargetToItem(int x, int y, int targetX, int targetY)
        {
            board.GetItem(x, y).TargetToMove = new Vector2Int(targetX, targetY);
            board.Cells[targetX, targetY].SetIsGettingFilled(true);
            board.GetItem(x, y).IsMoving = true;
            dirtyColumns[targetX] = true;
            board.Cells[x, y].SetIsGettingEmptied(true);

        }

        private void RemovePositionFromQueue(int x, int y)
        {
            board.Cells[x, y].SetIsGettingEmptied(false);
            if (!board.Cells[x, y].HasItem)
                return;
            Vector2Int target = board.GetItem(x, y).TargetToMove;
            board.GetItem(x, y).Transform.localPosition = LevelGrid.Instance.GetCellCenterLocalVector2(target);
            board.Cells[target.x,target.y].SetIsGettingFilled(false);
        }

        private bool CanFallToAnyDirection(int x, int y)
        {
            if (!CheckIfFallAble(x, y))
            {
                return false;
            }

            if (board.Cells[x, y - 1].CellType == CellType.SHIFTER)
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

            if (x < board.Width - 1 && CanFallDiagonally(x, y, x + 1))
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
                if (!board.Cells[x,y].HasItem && !board.Cells[x,y].IsLocked)
                    return true;
                if (board.Cells[x,y].IsGettingEmptied)
                    return true;
                if (board.Cells[x,y].HasItem&&board.Cells[x,y].BoardItem.IsExploding)
                    return true;
                if (board.Cells[x,y].HasItem && !board.GetItem(x, y).IsFallAble)
                    return false;
                y--;
            }
            return false;
        }
        private int HasAnyEmptyCellAfterShifter(int x, int y)
        {
            y--;
            while (y > 0&&board.Cells[x,y].CellType==CellType.SHIFTER)
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
            if (board.Cells[x,y-1].IsLocked)
                return false;

            return (!board.Cells[x,y-1].HasItem && !board.Cells[x,y-1].IsGettingFilled) ||
                   (board.Cells[x,y-1].HasItem && board.Cells[x,y-1].IsGettingEmptied);
        }

        private bool CheckIfFallAble(int x, int y)
        {
            if (!board.IsInBoundaries(x, y - 1)||board.Cells[x,y-1].CellType==CellType.BLANK)
                return false;
            return board.Cells[x,y].HasItem && board.GetItem(x, y).IsFallAble &&
                !board.Cells[x,y].BoardItem.IsSwapping || board.Cells[x,y].IsLocked;
        }

        private bool CanFallDiagonally(int x, int y, int targetX)
        {
            if (board.Cells[targetX, y - 1].HasItem || board.Cells[targetX, y - 1].IsLocked
                                                      ||board.Cells[targetX,y-1].CellType==CellType.BLANK
                                                      ||board.Cells[targetX,y-1].CellType==CellType.SHIFTER)
            {
                return false;
            }

            bool isFirstItemAboveObstacle = IsFirstItemAboveObstacle(targetX, y);
            if (!isFirstItemAboveObstacle)
            {
                return false;
            }
            if(y<board.Height-1&&board.Cells[x,y+1].HasItem&&board.GetItem(x,y+1).IsMoving&&board.GetItem(x,y+1).TargetToMove.x!=x)
                return false;

            bool canFall = !board.Cells[targetX, y-1].HasItem && !board.Cells[targetX, y-1].IsGettingFilled;
            return canFall;
        }

        private bool IsFirstItemAboveObstacle(int x, int y)
        {
            while (y < board.Height)
            {
                if (board.Cells[x, y].HasItem)
                {
                    return !board.Cells[x, y].BoardItem.IsFallAble;
                }
                y++;
            }

            return false;
        }

        private bool IsItemCloseToTarget(Vector3 currPos, Vector3 targetPos, float maxDistance = 0.05f)
        {
            return Mathf.Abs(targetPos.y - currPos.y) < maxDistance / 2 &&
                   Mathf.Abs(targetPos.x - currPos.x) < maxDistance / 2;
        }

        private void FinishItemMovement(int x, int y)
        {
            board.Cells[x, y].SetIsGettingEmptied(false);
            RemovePositionFromQueue(x, y);
            board.GetItem(x, y).IsMoving = CanFallToAnyDirection(x, y);
            if (board.GetItem(x, y).IsMoving) return;
            board.GetItem(x, y).FallSpeed = 1.5f;
            if(y<board.Height-1&&board.Cells[x,y+1].HasItem&&board.GetItem(x,y+1).IsMoving&&board.GetItem(x,y+1).ItemID==board.GetItem(x,y).ItemID)
                return;
            EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd, new Vector2Int(x, y));

        }

        #region Jester Hat Shuffle
            private void ShuffleItems()
            {
                List<Vector2Int> shuffleableCells = new List<Vector2Int>();
                for (int x = 0; x < board.Width; x++)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        if (board.Cells[x, y].HasItem && board.GetItem(x, y).IsShuffleAble)
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
                IBoardItem item1 = board.GetItem(pos1);
                IBoardItem item2 = board.GetItem(pos2);
                Vector3 item1Pos = item1.Transform.localPosition;
                Vector3 item2Pos = item2.Transform.localPosition;
                item1.Transform.DOLocalMove(item2Pos, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine);
                item2.Transform.DOLocalMove(item1Pos, 0.5f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete +=
                    () =>
                    {
                      IBoardItem temp = board.GetItem(pos1);
                        board.Cells[pos1.x, pos1.y].SetItem(board.GetItem(pos2));
                        board.Cells[pos2.x, pos2.y].SetItem(temp);
                        EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd, pos1);
                        EventManager.Instance.Broadcast(GameEvents.OnItemMovementEnd, pos2);
                    };
            }
        #endregion

    }
}
