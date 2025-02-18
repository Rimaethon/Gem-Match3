using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Data_Classes;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using Scripts;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Managers
{
    public class LevelManager:Singleton<LevelManager>
    {
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] private GameObject boardPrefab;
        [SerializeField] private GameObject backgroundPrefab;
        [SerializeField] private bool shouldGenerateRandomLevel;
        [SerializeField] private RandomLevelGenerator randomLevelGenerator;
        private Dictionary<int,List<IBoardItem>> _goalPositions = new Dictionary<int, List<IBoardItem>>();
        private Dictionary<int,int> _goalCounts= new Dictionary<int, int>();
        private readonly List<Board> _boards=new List<Board>();
        private int _moveCount=20;
        private bool _isLevelSet;
        private LevelData _levelData;
        public readonly HashSet<int> ItemsGettingMatchedByLightBall = new HashSet<int>();
        private const float BoardStretchAmount = -1f;
        private float initialXPos;
        public bool DoesBoardHasThingsToDo;
        private bool isLevelCompleted;
        private int normalHeight=10;
        private float _spriteMaskYOffsetForAdditionalHeight=0.5f;
        private int normalWidth=8;
        private float _boardXOffsetForAdditionalWidth=0.25f;

        private void OnEnable()
        {
            EventManager.Instance.AddHandler<Vector2Int,int>(GameEvents.OnItemExplosion, HandleItemExplosion);
            EventManager.Instance.AddHandler<int>(GameEvents.OnMoveCountChanged, HandleMoveCount);
        }
        private void OnDisable()
        {
            if(EventManager.Instance==null)
                return;
            EventManager.Instance.RemoveHandler<Vector2Int,int>(GameEvents.OnItemExplosion, HandleItemExplosion);
            EventManager.Instance.RemoveHandler<int>(GameEvents.OnMoveCountChanged, HandleMoveCount);
        }

        private void Start()
        {
            _levelData=SaveManager.Instance.GetCurrentLevelData();
            _isLevelSet = true;
            InitializeLevel().Forget();
        }
        //Actually It would be better to have a specified logic for where to put Cloche or user added boosters but it is not in the scope of this project
        private async UniTask InitializeLevel()
        {
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);

            SpriteRenderer backgroundSpriteRenderer= Instantiate(backgroundPrefab, Vector3.zero,Quaternion.identity).GetComponent<SpriteRenderer>();
            if(shouldGenerateRandomLevel)
            {
                GameObject boardInstance=Instantiate(boardPrefab, transform.position, Quaternion.identity);
                boardInstance.transform.SetParent(transform);
                Board board = randomLevelGenerator.GenerateRandomBoard(backgroundSpriteRenderer, boardInstance);
                _boards.Add(board);
                _goalPositions = randomLevelGenerator._goalPositions;
                _goalCounts = randomLevelGenerator._goalCounts;
                boardInstance.GetComponent<BoardManager>().InitializeBoard(board);
            }
            else if(_isLevelSet)
            {
                HashSet<int> spawnAbleItems =_levelData.SpawnAbleFillerItemIds.ToHashSet();
                spawnAbleItems.AddRange(_levelData.GoalSaveData.GoalIDs.ToList());
                backgroundSpriteRenderer.sprite= itemDatabase.Backgrounds[_levelData.backgroundID];
                await ObjectPool.Instance.InitializeStacks(spawnAbleItems,25,15);
                foreach (var  boardData in _levelData.Boards)
                {
                    GameObject boardInstance=Instantiate(boardPrefab, transform.position, Quaternion.identity);

                    boardInstance.transform.SetParent(transform);
                    Board board= new Board(itemDatabase.GetBoardSpriteData(boardData.BoardSpriteID),boardData,boardInstance,_levelData.SpawnAbleFillerItemIds);

                    _boards.Add(board);
                    boardInstance.GetComponent<SpriteRenderer>().sprite=itemDatabase.GetBoardSpriteData(boardData.BoardSpriteID).Sprite;
                    BoardManager boardManager= boardInstance.GetComponent<BoardManager>();
                    boardManager.InitializeBoard(board);
                    if(board.Height>normalHeight)
                        boardManager._spriteMask.transform.localPosition=
                            new Vector3(boardManager._spriteMask.transform.localPosition.x,boardManager._spriteMask.transform.localPosition.y+_spriteMaskYOffsetForAdditionalHeight*(board.Height-normalHeight),boardManager._spriteMask.transform.localPosition.z);
                    if(board.Width>normalWidth)
                        transform.position=new Vector3(transform.position.x-(_boardXOffsetForAdditionalWidth*(board.Width-normalWidth)),transform.position.y,transform.position.z);

                    randomLevelGenerator.InitializeGoalDictionaries(board,_levelData.GoalSaveData.GoalIDs.ToList(),_goalPositions,_goalCounts);
                    for(int i=0;i<_levelData.GoalSaveData.GoalAmounts.Length;i++)
                    {
                        _goalCounts[_levelData.GoalSaveData.GoalIDs[i]]=_levelData.GoalSaveData.GoalAmounts[i];
                    }
                }
                _moveCount = _levelData.MoveCount;
            }
            Vector3 boardInitialOffset= new Vector3(3f, transform.position.y, transform.position.z);

            initialXPos = transform.position.x;
            transform.position=boardInitialOffset;
            await InGameUIManager.Instance.HandleGoalAndPowerUpUI(_goalCounts,_moveCount);
            await HandleBoardAnimation();
        }
        private async UniTask HandleBoardAnimation()
        {
            await UniTask.Delay(200);
            await transform.DOMoveX(initialXPos + BoardStretchAmount, 0.4f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).ToUniTask();
            await transform.DOMoveX(initialXPos, 0.2f).SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).ToUniTask();
            await UniTask.Delay(200);
            List<int> boostersUsedThisLevel=SceneController.Instance.GetBoostersUsedThisLevel();
            if (boostersUsedThisLevel != null)
            {
                HashSet<Vector2Int> spawnablePositions = GetRandomSpawnablePos(boostersUsedThisLevel.Count);

                foreach (var boosterID in boostersUsedThisLevel )
                {
                    Vector2Int spawnPos = spawnablePositions.First();
                    EventManager.Instance.Broadcast(GameEvents.OnItemRemoval, spawnPos);
                    EventManager.Instance.Broadcast<Vector2Int,int>(GameEvents.AddItemToAddToBoard, spawnPos, boosterID);
                    spawnablePositions.Remove(spawnPos);
                    await UniTask.Delay(200);
                }
                boostersUsedThisLevel.Clear();
            }
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
        }

        private void HandleMoveCount(int valueToAdd)
        {
            _moveCount+=valueToAdd;
            if (_moveCount > 0) return;
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
            WaitForBoardToFinish().Forget();
        }
        private async UniTask WaitForBoardToFinish()
        {
            await UniTask.Delay(300);
            while (DoesBoardHasThingsToDo)
            {
                await UniTask.Delay(300);
            }
            await UniTask.Delay(400);

            if (CheckForLevelCompletion())
            {
                EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
                return;
            }
            AudioManager.Instance.PlaySFX(SFXClips.LevelLoseSound);
            EventManager.Instance.Broadcast(GameEvents.OnNoMovesLeft);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
        }
        public bool IsGoalReached(int itemID)
        {
            return _goalCounts[itemID] <= 0;
        }
        private void HandleItemExplosion(Vector2Int pos,int itemID)
        {
            if (_goalCounts.ContainsKey(itemID))
            {
                IBoardItem boardItem;
                if (_boards[0].Cells[pos.x, pos.y].HasItem && _boards[0].Cells[pos.x, pos.y].BoardItem.ItemID == itemID)
                {
                    boardItem = _boards[0].Cells[pos.x, pos.y].BoardItem;
                }else if (_boards[0].Cells[pos.x, pos.y].HasUnderLayItem && _boards[0].Cells[pos.x, pos.y].UnderLayBoardItem.ItemID == itemID)
                {
                    boardItem = _boards[0].Cells[pos.x, pos.y].UnderLayBoardItem;
                }
                else
                {
                    return;
                }
                if (_goalCounts[itemID] > 0)
                {
                    _goalCounts[itemID]--;
                    if (!boardItem.IsGeneratorItem)
                    {
                        _goalPositions[boardItem.ItemID].Remove(boardItem);
                    }
                    EventManager.Instance.Broadcast<int,int>(GameEvents.OnGoalUIUpdate, boardItem.ItemID, _goalCounts[boardItem.ItemID]);
                    if (_goalCounts[boardItem.ItemID] == 0)
                    {
                        if (boardItem.IsGeneratorItem)
                        {
                            _goalPositions[boardItem.ItemID].Clear();
                        }
                        CheckForLevelCompletion();

                    }

                }
            }

        }
        private bool CheckForLevelCompletion()
        {
            if(isLevelCompleted)
                return true;
            if (_goalCounts.Values.All(count => count == 0))
            {
                isLevelCompleted = true;
                HandleCompletion().Forget();
                return true;
            }

            return false;

        }
        private async UniTask HandleCompletion()
        {
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputLock);
            EventManager.Instance.Broadcast(GameEvents.OnBoardLock);
            await UniTask.Delay( 500);
            while (DoesBoardHasThingsToDo)
            {
                await UniTask.Delay( 200);
            }
            await UniTask.Delay( 500);
            EventManager.Instance.Broadcast(GameEvents.OnLevelCompleted);
            EventManager.Instance.Broadcast(GameEvents.OnPlayerInputUnlock);
        }

        //To Accommodate Power Up usage in multiple board levels I believe its best to put these methods here instead of BoardManager
        public bool IsValidPosition(Vector2Int pos)
        {
            foreach (var board in _boards)
            {
                if (board.IsInBoundaries(pos))
                {

                    return true;
                }
            }

            return false;
        }
        public Board GetBoard(Vector2Int pos)
        {
            foreach (var board in _boards)
            {
                if (board.IsInBoundaries(pos))
                {
                    return board;
                }
            }

            return null;
        }
        public void CheckHammerHit(Vector2Int pos)
        {
            foreach (var board in _boards)
            {
                if (!board.IsInBoundaries(pos)) continue;
                if (board.Cells[pos.x, pos.y].HasItem)
                {

                    IBoardItem item = board.GetItem(pos);
                    item.OnExplode();
                }
            }
        }

        //https://youtu.be/iSaTx0T9GFw?t=3697 as can be seen in here cells that has underlay item is considered valid position so I will do the same.
        public HashSet<Vector2Int> GetRandomSpawnablePos(int numberOfPositions)
        {
            int maxTries = 30 * numberOfPositions; // Increase max tries based on number of positions needed
            HashSet<Vector2Int> spawnablePositions = new HashSet<Vector2Int>(); // Use HashSet to ensure uniqueness

            foreach (var board in _boards)
            {
                while (maxTries > 0 && spawnablePositions.Count < numberOfPositions)
                {
                    maxTries--;
                    Cell cell = board.Cells[Random.Range(0, board.Width), Random.Range(0, board.Height)];
                    if (cell.HasItem && cell.BoardItem.IsShuffleAble && !cell.HasOverLayItem&&!cell.BoardItem.IsMoving && !cell.BoardItem.IsBooster&&
                        !cell.BoardItem.IsExploding && !cell.BoardItem.IsMoving &&
                        !cell.IsLocked && !cell.BoardItem.IsMatching&&cell.BoardItem.IsActive)
                    {
                        spawnablePositions.Add(cell.CellPosition); // Add position to HashSet
                    }
                }
            }
            if (spawnablePositions.Count < numberOfPositions)
            {
                Debug.LogWarning("Could not find enough unique spawnable positions.");
            }
            return spawnablePositions;
        }
        //Actually this part deserves its own class with specialized methods for finding suitable goal areas for TNT and rockets such as finding most goals in a column for vertical rocket.
        //So that player doesn't feel like missile is making bad and unpredictable decisions
        public Vector2Int GetRandomGoalPos()
        {
            foreach (var goalList in  _goalPositions.Values)
            {
               if (goalList.Count > 0)
               {
                   Vector2Int goalPos = goalList[UnityEngine.Random.Range(0, goalList.Count)].Position;
                   return goalPos;
               }
            }
            foreach (var board in _boards)
            {
                foreach (var cell in board.Cells)
                {
                    if(cell.HasItem)
                        return cell.CellPosition;
                }
            }
            return new Vector2Int(-1, -1);
        }
    }
}
