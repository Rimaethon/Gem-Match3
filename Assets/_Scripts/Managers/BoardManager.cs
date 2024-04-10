using System.Collections.Generic;
using System.Linq;
using _Scripts.Core;
using _Scripts.Managers.Matching;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using Data;
using Rimaethon.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
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
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int itemTypeStart;
        [SerializeField] private int itemTypeEnd=4;
        
        private bool _isBoardShaking;
        private const float ShakeDuration = 0.5f;
        private const float ShakeMagnitude = 0.1f;

        
        [SerializeField] private bool[] dirtyColumns;
        private Board _board;
        private SwipeController _swipeController;
        private MatchChecker _matchChecker;
        private RandomBoardGenerator _randomBoardGenerator;
        private ItemMovement _itemMovement;
        private MatchHandler _matchHandler;
        private ItemRemovalHandler _itemRemovalHandler;
        private SpawnHandler _spawnHandler;
        private ItemActionHandler _itemActionHandler;
     


     
        private void OnEnable()
        {
            if(EventManager.Instance==null)
                return;
            EventManager.Instance.AddHandler(GameEvents.OnBoardShake, HandleBoardShake);
        }
        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnBoardShake, HandleBoardShake);
        }
    
        private void Start()
        {
            Application.targetFrameRate = 60;
            dirtyColumns = new bool[width];
            _randomBoardGenerator = new RandomBoardGenerator();
            _board =_randomBoardGenerator.GenerateRandomBoard(width, height,itemTypeStart,itemTypeEnd);
            _matchChecker = new MatchChecker(_board,width,height);
            _itemMovement = new ItemMovement(_board,dirtyColumns,width,height);
            _swipeController = new SwipeController(_board,_matchChecker,width,height);
            _matchHandler = new MatchHandler(_board,dirtyColumns);
            _itemRemovalHandler = new ItemRemovalHandler(_board);
            _spawnHandler = new SpawnHandler(_board);
            _itemActionHandler = new ItemActionHandler(_board);
            
            EventManager.Instance.Broadcast<Board>(GameEvents.FindGoals,_board);
        }
  
        private void FixedUpdate()
        {
            _itemMovement.MoveItems();
            _swipeController.MakeSwipe();
            _matchChecker.CheckForMatches();
            _matchHandler.HandleMatches();
            _itemRemovalHandler.HandleItemRemoval();
            _spawnHandler.HandleBoosterSpawn();
            _spawnHandler.HandleFillSpawn();
            _itemActionHandler.HandleActions();
        }
 
        private void HandleBoardShake()
        {
            if (_isBoardShaking)
                return;
            BoardShake().Forget();
        }
        private async UniTaskVoid BoardShake()
        {
            _isBoardShaking = true;
            Vector3 originalPosition = transform.position;
            float elapsed = 0.0f;
            while (elapsed < ShakeDuration)
            {
                float x = Random.Range(-1f, 1f) * ShakeMagnitude;
                float y = Random.Range(-1f, 1f) * ShakeMagnitude;
                transform.position = new Vector3(originalPosition.x+x,originalPosition.y+ y, originalPosition.z);
                elapsed += Time.deltaTime;
                await UniTask.Yield();
            }
            transform.position = originalPosition;
            _isBoardShaking = false;
        }
    }
}
