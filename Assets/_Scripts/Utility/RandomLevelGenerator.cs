using System;
using System.Collections.Generic;
using _Scripts.Data_Classes;
using Scripts;
using Sirenix.OdinInspector;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _Scripts.Utility
{
    public class RandomLevelGenerator:MonoBehaviour
    {
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] private Vector3 boardPosition;
        private int _boardID;
        private int boardSpriteID;
        [ValueDropdown("GetBoardSpriteID")]
        [SerializeField] private Sprite boardSprite;
        [ValueDropdown("GetBackgroundSprite")]
        [SerializeField] private Sprite backgroundSprite;
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> normalItemTypesToSpawn = new List<int>();
        [SerializeField] private int totalOverlayItemCount;
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> overlayItemTypesToSpawn = new List<int>();
        [SerializeField] private int totalUnderlayItemCount;
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> underlayItemTypesToSpawn = new List<int>();
        [Tooltip("The counts will be depending on random generation")]
        [ValueDropdown("GetNormalItemIds")]
        [SerializeField] private List<int> goalIds = new List<int>();
        public Dictionary<int,List<IBoardItem>> _goalPositions = new Dictionary<int, List<IBoardItem>>();
        public Dictionary<int,int> _goalCounts= new Dictionary<int, int>();
        private List<int> _spawnAbleFillerItemIds=new List<int>(){0,1,2,3,4};
        private IEnumerable<ValueDropdownItem<int>> GetNormalItemIds()
        {
            foreach (var item in itemDatabase.NormalItems)
            {
                yield return new ValueDropdownItem<int>(item.Value.ItemPrefab.name, item.Key);
            }
        }
        private IEnumerable<ValueDropdownItem<Sprite>> GetBoardSpriteID()
        {
            foreach (var item in itemDatabase.Boards)
            {
                _boardID = item.Key;
                yield return new ValueDropdownItem<Sprite>(item.Key.ToString(), item.Value.Sprite);
            }
        }
        private IEnumerable<ValueDropdownItem<Sprite>> GetBackgroundSprite()
        {
            foreach (var item in itemDatabase.Backgrounds)
            {

                yield return new ValueDropdownItem<Sprite>(item.Value.name, item.Value);
            }
        }

         [BurstCompile]
        public Board GenerateRandomBoard(SpriteRenderer backgroundSpriteRenderer,GameObject boardInstance)
        {
            Random random = new Random((uint)DateTime.Now.Ticks);
            BoardSpriteSaveData boardSpriteSaveData= itemDatabase.Boards[_boardID];
            var job = new GenerateBoardJob
            {
                Width = boardSpriteSaveData.Width,
                Height = boardSpriteSaveData.Height,
                Board = new NativeArray<int>(boardSpriteSaveData.Width * boardSpriteSaveData.Height, Allocator.TempJob),
                Random = random,
                NormalItemTypes = normalItemTypesToSpawn.ToNativeArray(Allocator.TempJob),

            };
            job.Schedule().Complete();
            int[,] normalItemArray =Create2DArrayFromNativeArray(job.Board, boardSpriteSaveData.Width, boardSpriteSaveData.Height);
            Dictionary<Vector2Int,int> underlayItemIds= GenerateItemsAtRandomPositions(boardSpriteSaveData,underlayItemTypesToSpawn,totalUnderlayItemCount);
            Dictionary<Vector2Int,int> overlayItemIds= GenerateItemsAtRandomPositions(boardSpriteSaveData,overlayItemTypesToSpawn,totalOverlayItemCount);
            BoardData boardData = new BoardData(_boardID,boardPosition,normalItemArray,underlayItemIds,overlayItemIds);
            Board board = new Board(boardSpriteSaveData,boardData,boardInstance,_spawnAbleFillerItemIds);
            InitializeGoalDictionaries(board, goalIds, _goalPositions, _goalCounts);
            backgroundSpriteRenderer.sprite = backgroundSprite;
            job.Board.Dispose();
            job.NormalItemTypes.Dispose();
            return board;
        }

        [BurstCompile]
        struct GenerateBoardJob : IJob
        {
            public int Height;
            public int Width;
            public NativeArray<int> Board;
            public Random Random;
            public NativeArray<int> NormalItemTypes;
            public void Execute()
            {
                do
                {
                    GenerateBoardWithNoTriplets(Width,Height, Random,NormalItemTypes, ref Board);
                } while (!HasMatchableSwap(Board,Width,Height));
            }
        }
        [BurstCompile]
        private static void GenerateBoardWithNoTriplets(int width, int height, Random random, NativeArray<int> itemTypes,ref NativeArray<int> board)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int randomIndex = itemTypes[random.NextInt(0, itemTypes.Length)];
                board[y * width + x] = randomIndex;

                // Check for triplets and square matches and change the cell type if there is a match
                while ((y >= 1 && x >= 1 && board[(y - 1) * width + x] == randomIndex && board[y * width + x - 1] == randomIndex &&
                        board[(y - 1) * width + x - 1] == randomIndex) ||
                       (x >= 2 && board[y * width + x - 1] == randomIndex && board[y * width + x - 2] == randomIndex) ||
                       (y >= 2 && board[(y - 1) * width + x] == randomIndex && board[(y - 2) * width + x] == randomIndex))
                {
                    randomIndex = itemTypes[random.NextInt(0, itemTypes.Length)];
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

        private Dictionary<Vector2Int, int> GenerateItemsAtRandomPositions(BoardSpriteSaveData spriteSaveData,List<int> itemIDs,int maxItem)
        {
            Dictionary<Vector2Int, int> itemPositions = new Dictionary<Vector2Int, int>();
            int maxTries = 100;

            while(maxItem>0)
            {
                int x = UnityEngine.Random.Range(0, spriteSaveData.Width);
                int y = UnityEngine.Random.Range(0, spriteSaveData.Height);
                Vector2Int pos = new Vector2Int(x, y);
                if ( itemPositions.ContainsKey(pos))
                {
                    maxTries--;
                    if (maxTries <= 0)
                    {
                        break;
                    }
                    continue;
                }

                itemPositions.Add(pos, itemIDs[UnityEngine.Random.Range(0, itemIDs.Count)]);
                maxItem--;
            }

            return itemPositions;
        }
        public  void InitializeGoalDictionaries(Board board, List<int> goalIds, Dictionary<int, List<IBoardItem>> goalPositions, Dictionary<int, int> goalCounts)
        {
            foreach (var goalId in goalIds)
            {
                goalPositions.Add(goalId, new List<IBoardItem>());
                goalCounts.Add(goalId, 0);
            }
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.Cells[x, y].HasItem && goalIds.Contains(board.Cells[x, y].BoardItem.ItemID))
                    {
                        goalPositions[board.Cells[x, y].BoardItem.ItemID].Add(board.Cells[x, y].BoardItem);
                        goalCounts[board.Cells[x, y].BoardItem.ItemID]++;
                    }
                    if(board.Cells[x, y].HasUnderLayItem && goalIds.Contains(board.Cells[x, y].UnderLayBoardItem.ItemID))
                    {
                        goalPositions[board.Cells[x, y].UnderLayBoardItem.ItemID].Add(board.Cells[x, y].UnderLayBoardItem);
                        goalCounts[board.Cells[x, y].UnderLayBoardItem.ItemID]++;
                    }
                    if(board.Cells[x, y].HasOverLayItem && goalIds.Contains(board.Cells[x, y].OverLayBoardItem.ItemID))
                    {
                        goalPositions[board.Cells[x, y].OverLayBoardItem.ItemID].Add(board.Cells[x, y].OverLayBoardItem);
                        goalCounts[board.Cells[x, y].OverLayBoardItem.ItemID]++;
                    }

                }

            }
        }
        private static int[,] Create2DArrayFromNativeArray(NativeArray<int> array, int width, int height)
        {
            int[,] result = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = array[y * width + x];

                }

            }
            return result;
        }
    }
}
