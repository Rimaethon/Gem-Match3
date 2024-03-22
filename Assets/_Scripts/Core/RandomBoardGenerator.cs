using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Timeline;
using Random = Unity.Mathematics.Random;

namespace _Scripts.Core
{
    // This class can also be adjusted for making levels harder with generating less one swap matchable doublets
    public class RandomBoardGenerator
    {
        static readonly ProfilerMarker myMarker = new ProfilerMarker("GenerateRandomBoard");
        [BurstCompile]
        public int[,] GenerateRandomBoard(int width=8, int height=10,int minTypeID=0,int maxTypeID=4)
        {
            var job = new GenerateBoardJob
            {
                Width = width,
                Height = height,
                Board = new NativeArray<int>(width * height, Allocator.TempJob),
                Random = new Random((uint)DateTime.Now.Ticks),
                MinTypeID = minTypeID,
                MaxTypeID = maxTypeID,
                Marker = myMarker,
            };

            job.Schedule().Complete();

            int[,] result = new int[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    result[i, j] = job.Board[j * width + i];
                }
            }

            job.Board.Dispose();
            return result;
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
            
            public ProfilerMarker Marker;
            public void Execute()
            {
                Marker.Begin();
                do
                {
                    GenerateBoardWithNoTriplets(Width,Height, Random,MinTypeID,MaxTypeID, ref Board);
                } while (!HasMatchableSwap(Board,Width,Height));
                Marker.End();
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
        
 
    
         
        [BurstCompile]
public static string HasShape(NativeArray<int> board, int x, int y, int width, int height)
{
    int value = board[y * width + x];

    // Check for square
    if (x < width - 1 && y < height - 1 && board[(y * width) + x + 1] == value && board[((y + 1) * width) + x] == value && board[((y + 1) * width) + x + 1] == value)
    {
        return "Square";
    }

    // Check for triplet
    if ((x >= 2 && board[y * width + x - 1] == value && board[y * width + x - 2] == value) ||
        (x < width - 2 && board[y * width + x + 1] == value && board[y * width + x + 2] == value) ||
        (y >= 2 && board[(y - 1) * width + x] == value && board[(y - 2) * width + x] == value) ||
        (y < height - 2 && board[(y + 1) * width + x] == value && board[(y + 2) * width + x] == value))
    {
        // Check for quadruplet
        if ((x >= 3 && board[y * width + x - 3] == value) ||
            (x < width - 3 && board[y * width + x + 3] == value) ||
            (y >= 3 && board[(y - 3) * width + x] == value) ||
            (y < height - 3 && board[(y + 3) * width + x] == value))
        {
            // Check for L shape
            if ((x >= 2 && y < height - 2 && board[(y + 1) * width + x] == value && board[(y + 2) * width + x] == value) ||
                (x < width - 2 && y < height - 2 && board[(y + 1) * width + x] == value && board[(y + 2) * width + x] == value) ||
                (x >= 2 && y >= 2 && board[(y - 1) * width + x] == value && board[(y - 2) * width + x] == value) ||
                (x < width - 2 && y >= 2 && board[(y - 1) * width + x] == value && board[(y - 2) * width + x] == value))
            {
                return "L";
            }

            // Check for T shape
            if (x >= 1 && x < width - 1 && y < height - 2 && board[(y + 1) * width + x - 1] == value && board[(y + 1) * width + x + 1] == value)
            {
                return "T";
            }

            return "Quadruplet";
        }

        return "Triplet";
    }

    return "None";
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