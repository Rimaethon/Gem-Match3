using System;
using _Scripts.Utility;
using Scripts;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _Scripts.Core
{
    // This class can also be adjusted for making levels harder with generating less one swap matchable doublets
    public class RandomBoardGenerator
    {
        [BurstCompile]
        public Board GenerateRandomBoard(int width=8, int height=10,int minTypeID=0,int maxTypeID=4)
        {
            var job = new GenerateBoardJob
            {
                Width = width,
                Height = height,
                Board = new NativeArray<int>(width * height, Allocator.TempJob),
                Random = new Random((uint)DateTime.Now.Ticks),
                MinTypeID = minTypeID,
                MaxTypeID = maxTypeID,
            };
            job.Schedule().Complete();
            int[,] itemTypeArray = new int[width, height];
            Board board = new Board(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    itemTypeArray[x, y] = job.Board[y * width + x];
                    board.GetCell(x, y).SetItem( ObjectPool.Instance
                        .GetItemGameObject(itemTypeArray[x, y], LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(x, y, 0)),board));
                    board.GetCell(x,y).UnderLayItem=ObjectPool.Instance.GetItemGameObject(6, LevelGrid.Grid.GetCellCenterWorld(new Vector3Int(x, y, 0)),board);
                    board.GetItem(x,y).Transform.parent = LevelGrid.Grid.transform;
                    board.GetCell(x,y).UnderLayItem.Transform.parent = LevelGrid.Grid.transform;
                }
            }
       
            job.Board.Dispose();
            return board;
        }
        
        [BurstCompile]
        struct GenerateBoardJob : IJob
        {
            public int Height;
            public int Width;
            public NativeArray<int> Board;
            public Random Random;
            public int MinTypeID;
            public int MaxTypeID;
            public void Execute()
            {
                do
                {
                    GenerateBoardWithNoTriplets(Width,Height, Random,MinTypeID,MaxTypeID, ref Board);
                } while (!HasMatchableSwap(Board,Width,Height));
            }
        }
        [BurstCompile]
        private static void GenerateBoardWithNoTriplets(int width, int height, Random random,int minTypeID,int maxTypeID, ref NativeArray<int> board)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int randomIndex = random.NextInt(minTypeID, maxTypeID);
                board[y * width + x] = randomIndex;

                // Check for triplets and square matches and change the cell type if there is a match
                while ((y >= 1 && x >= 1 && board[(y - 1) * width + x] == randomIndex && board[y * width + x - 1] == randomIndex &&
                        board[(y - 1) * width + x - 1] == randomIndex) ||
                       (x >= 2 && board[y * width + x - 1] == randomIndex && board[y * width + x - 2] == randomIndex) ||
                       (y >= 2 && board[(y - 1) * width + x] == randomIndex && board[(y - 2) * width + x] == randomIndex))
                {
                    randomIndex = random.NextInt(minTypeID, maxTypeID);
                    board[y * width + x] = randomIndex;
                }
            }
        }
        
        // Check if there is a matchable swap with swapping every two adjacent cells and checking for triplets
        [BurstCompile]
        private static bool HasMatchableSwap(NativeArray<int> board,int width, int height)
        {
            for (int y = 0; y < width; y++)
            for (int x = 0; x < height; x++)
            for (int i = 0; i < 4; i++)
            {
                //swap left right up down logic since there are 4 directions to check I'm iterating 4 times
                int dx = i == 0 ? 0 : i == 1 ? 1 : i == 2 ? 0 : -1;
                int dy = i == 0 ? 1 : i == 1 ? 0 : i == 2 ? -1 : 0;
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                // Swap
                int temp = board[y * width + x];
                board[y * width + x] = board[ny * width + nx];
                board[ny * width + nx] = temp;

                // Check for triplet
                if (HasTriplet(board, x, y, width,height) || HasTriplet(board, nx, ny,width, height))
                {
                    // Swap back
                    board[ny * width + nx] = board[y * width + x];
                    board[y * width + x] = temp;
                    
                    return true;
                }

                // Swap back
                board[ny * width + nx] = board[y * width + x];
                board[y * width + x] = temp;
            }

            return false;
        }
        // Check if there is a triplet at the given cell
        [BurstCompile]
        private static bool HasTriplet(NativeArray<int> board, int x, int y, int width, int height)
        {
            int value = board[y * width + x];
            return (x >= 2 && board[y * width + x - 1] == value && board[y * width + x - 2] == value) ||
                   (x < width - 2 && board[y * width + x + 1] == value && board[y * width + x + 2] == value) ||
                   (y >= 2 && board[(y - 1) * width + x] == value && board[(y - 2) * width + x] == value) ||
                   (y < height - 2 && board[(y + 1) * width + x] == value && board[(y + 2) * width + x] == value);
        }
    }
}