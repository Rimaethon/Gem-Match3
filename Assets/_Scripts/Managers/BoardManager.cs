using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Core;
using _Scripts.Managers;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Data;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using Random = UnityEngine.Random;

//Bomb + rocket = triple rocket vertical and horizontal 
// LightBall + Any Booster= spawning that booster 10 times and also converting the matched position to that booster 
//propeller+rocket||TNT= matching that booster near to goal also with matching 4 items on all directions at position of merge. 
//propeller+propeller=triple propeller 
//TNT+TNT || LIGHTBALL+LIGHTBALL= ALL BOARD EXPLOSION
//ALL ITEMS MERGE AT SWIPED POSITION SO swapping position gets empty before anything happens and blocks can fall to that position
//diagonal fall logic : If the first thing on the empty positions or up is an obstacle and my up or left is empty or has an obstacle then I can fall diagonally 
namespace Scripts
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private bool[] dirtyColumns;
        private Board _board;
        private InputHandler _inputHandler;
        private MatchChecker _matchChecker;
        private ItemMovement _itemMovement;
        private MatchHandler _matchHandler;
        private ItemRemovalHandler _itemRemovalHandler;
        private SpawnHandler _spawnHandler;
        private ItemActionHandler _itemActionHandler;
        private bool _isBoardInitialized;
        private bool _isBoardShaking;
        private const float ShakeDuration = 0.2f;
        private const float ShakeMagnitude = 0.06f;
        private bool hasActionToRun;
        private bool hasMatchToRun;
        private bool hasItemToRemove;
        private bool hasFillToSpawn;
        private bool hasItemToMove;
        private void OnEnable()
        {
            GC.Collect();
            if(EventManager.Instance==null)
                return;
            EventManager.Instance.AddHandler(GameEvents.OnBoardShake, HandleBoardShake);
        }

        private void OnDisable()
        {
            _inputHandler.OnDisable();
            _itemMovement.OnDisable();
            _matchChecker.OnDisable();
            _matchHandler.OnDisable();
            _itemRemovalHandler.OnDisable();
            _spawnHandler.OnDisable();
            _itemActionHandler.OnDisable();
            _inputHandler = null;
            _itemMovement = null;
            _matchChecker = null;
            _matchHandler = null;
            _itemRemovalHandler = null;
            _spawnHandler= null;
            _itemActionHandler = null;
            _board = null;
            GC.Collect();
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnBoardShake, HandleBoardShake);
        }
        
        public void InitializeBoard(Board board)
        {
            _board = board;
            _board.SetBoardItemsParent(gameObject.transform);
            dirtyColumns = new bool[_board.Width];
            _matchChecker = new MatchChecker(_board);
            _itemMovement = new ItemMovement(_board,dirtyColumns);
            _inputHandler = new InputHandler(_board,_matchChecker);
            _matchHandler = new MatchHandler(_board);
            _itemRemovalHandler = new ItemRemovalHandler(_board,dirtyColumns);
            _spawnHandler = new SpawnHandler(gameObject,_board,dirtyColumns,_board._spawnAbleFillerItemIds);
            _itemActionHandler = new ItemActionHandler(_board);
            _isBoardInitialized = true;
        }


        private void FixedUpdate()
        {
            if (!_isBoardInitialized)
                return;
            hasItemToMove=_itemMovement.MoveItems();
            _inputHandler.HandleInputs();
            _matchChecker.CheckForMatches();
            hasMatchToRun=_matchHandler.HandleMatches();
            hasItemToRemove= _itemRemovalHandler.HandleItemRemoval();
            _spawnHandler.HandleBoosterSpawn();
            hasFillToSpawn=_spawnHandler.HandleFillSpawn();
            hasActionToRun=_itemActionHandler.HandleActions();
            LevelManager.Instance.DoesBoardHasThingsToDo = hasItemToMove || hasMatchToRun || hasItemToRemove || hasFillToSpawn || hasActionToRun;
            
        }
        private void HandleBoardShake()
        {
            if (_isBoardShaking||!_isBoardInitialized)
                return;
            transform.DOShakePosition(ShakeDuration, ShakeMagnitude, 10, 90, false).SetUpdate(UpdateType.Fixed).OnComplete(() =>
            {
                _isBoardShaking = false;
                transform.localPosition = Vector3.zero;
            });
        }
    }
}
