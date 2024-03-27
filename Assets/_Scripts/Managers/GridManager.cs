using System.Collections.Generic;
using _Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;
using Vector3Int = UnityEngine.Vector3Int;

namespace Scripts
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        //Pool is only needed for the grid so I'm passing it with serialized field
        [SerializeField] private ObjectPool _objectPool;
        private Camera _camera;
        private bool _isLerping;
        private int[,] gameBoard;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float lerpSpeed=0.2f;

        private IItem[,] _tileItems;
        //Im only using local position to 
        private Vector2[,] _cellPositions;
        
        private HashSet<Vector3Int> lerpingItems=new HashSet<Vector3Int>();
        private RandomBoardGenerator _randomBoardGenerator;
        
        
        private void OnEnable()
        {
            EventManager.Instance.AddHandler<Vector3Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.AddHandler<List<Vector3Int>>(GameEvents.OnHorizontalMatch, HorizontalMatch);
            EventManager.Instance.AddHandler<List<Vector3Int>>(GameEvents.OnBomb, BombMatch);
            EventManager.Instance.AddHandler<Vector2>(GameEvents.OnTouch, OnTouch);
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler<Vector3Int, Vector2>(GameEvents.OnSwipe, OnSwipe);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnClick, OnClick);
            EventManager.Instance.RemoveHandler<List<Vector3Int>>(GameEvents.OnHorizontalMatch, HorizontalMatch);
            EventManager.Instance.RemoveHandler<List<Vector3Int>>(GameEvents.OnBomb, BombMatch);
            EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnTouch, OnTouch);
        }

        private async void HorizontalMatch(List<Vector3Int> elements)
        {
            await HandleMatchWithDelay(elements);
        }
        private async void BombMatch(List<Vector3Int> elements)
        {
             await HandleBombMatch(elements);
        }

        
        private void OnTouch(Vector2 touchPos)
        {
            Vector3Int firstCellPos = grid.WorldToCell(touchPos);
            if (IsInsideGrid(firstCellPos)&&_tileItems[firstCellPos.x, firstCellPos.y]!=null)
            {
                _tileItems[firstCellPos.x, firstCellPos.y].OnTouch();
            }
        }
        
        private bool IsInsideGrid(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }
    
        private void Awake()
        {

            _camera = Camera.main;
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            _randomBoardGenerator = new RandomBoardGenerator();
            gameBoard = _randomBoardGenerator.GenerateRandomBoard(width, height);
            Generate();
        }

        private void Generate()
        {

            _tileItems = new IItem[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    
                    _tileItems[i,j] =_objectPool.GetItemFromPool(gameBoard[i, j], grid.GetCellCenterWorld(new Vector3Int(i, j, 0))).GetComponent<IItem>();
                    
                    _tileItems[i, j].Transform.parent = transform;
                }
                

            }
          
        }


        private async void OnClick(Vector2 clickPos)
        {
            Vector3Int firstCellPos = grid.WorldToCell(clickPos);
            if (!IsDraggable(firstCellPos))
                return;
            _tileItems[firstCellPos.x,firstCellPos.y].OnClick(_tileItems,new Vector2Int(firstCellPos.x,firstCellPos.y));
            OnTouch(clickPos);
        }

        private bool isSwiping=false;
        private async void  OnSwipe(Vector3Int direction, Vector2 clickPos)
        {

            if (isSwiping)
            {
                return;
            }
            Vector3Int firstCellPos = grid.WorldToCell(clickPos);
            Vector3Int secondCellPos = firstCellPos + direction;
          
            if (!IsDraggable(firstCellPos) || !IsDraggable(secondCellPos))
                return;
            isSwiping = true;

            Debug.Log(lerpingItems.Contains(firstCellPos));
            lerpingItems.Add(firstCellPos);
            lerpingItems.Add(secondCellPos);    
            _tileItems[firstCellPos.x, firstCellPos.y].SortingOrder= 5;
            _tileItems[secondCellPos.x, secondCellPos.y].SortingOrder = 4;
            await ChangeItemPositions(firstCellPos, secondCellPos); 
            bool pos2HasMatch = await HasMatch(firstCellPos);
            bool pos1HasMatch = await HasMatch(secondCellPos);
            if (!pos1HasMatch&& !pos2HasMatch)
            {
                await ChangeItemPositions(secondCellPos, firstCellPos); 
            }
            isSwiping = false;
            OnTouch(clickPos);
            
        }
        private bool IsDraggable(Vector3Int pos)
        {
            return  pos is { x: >= 0, y: >=0 } && pos.x<width && pos.y<height&&_tileItems[pos.x,pos.y]!=null&&_tileItems[pos.x, pos.y].IsDraggable&& !lerpingItems.Contains(pos)&&_tileItems[pos.x, pos.y].IsMovable;
        }

        private async UniTask ChangeItemPositions(Vector3Int pos1, Vector3Int pos2, float lerpSpeed = 0.15f)
        {
            Vector3 pos1InitialPosition = grid.GetCellCenterLocal(pos1);
            Vector3 pos2InitialPosition = grid.GetCellCenterLocal(pos2);
            Sequence sequence = DOTween.Sequence();
            sequence.Insert(0,_tileItems[pos1.x, pos1.y].Transform.DOLocalMove(pos2InitialPosition, lerpSpeed));
            sequence.Insert(0,_tileItems[pos2.x, pos2.y].Transform.DOLocalMove(pos1InitialPosition, lerpSpeed));

            await sequence.AwaitForComplete();    
            SwapItemsInArray(pos1, pos2);
          
           
        }
      
        private void SwapItemsInArray(Vector3Int pos1, Vector3Int pos2)
        {
            (_tileItems[pos1.x, pos1.y], _tileItems[pos2.x, pos2.y]) = (_tileItems[pos2.x, pos2.y], _tileItems[pos1.x, pos1.y]);
        }
        
    
        public static List<Vector3Int> CheckHorizontalMatches(IItem[,] board, int itemX, int itemY, int width, int height)
        {
            List<Vector3Int> matchedPositions = new List<Vector3Int>();
            IItem currentItem = board[itemX, itemY];

            // Check to the right
            for (int x = itemX; x < width; x++)
            {
                if (board[x, itemY] != null && board[x, itemY].ItemType == currentItem.ItemType)
                {
                    matchedPositions.Add(new Vector3Int(x, itemY, 0));
                }
                else
                {
                    break;
                }
            }
            
            // Check to the left
            for (int x = itemX - 1; x >= 0; x--)
            {
                if (board[x, itemY] != null && board[x, itemY].ItemType == currentItem.ItemType)
                {
                    matchedPositions.Add(new Vector3Int(x, itemY, 0));
                }
                else
                {
                    break;
                }
            }

            return matchedPositions.Count >= 3 ? matchedPositions : new List<Vector3Int>();
        }

        public static List<Vector3Int> CheckVerticalMatches(IItem[,] board, int itemX, int itemY, int width, int height)
        {
            List<Vector3Int> matchedPositions = new List<Vector3Int>();
            IItem currentItem = board[itemX, itemY];

            // Check upwards
            for (int y = itemY; y < height; y++)
            {
                if (board[itemX, y] != null && board[itemX, y].ItemType == currentItem.ItemType)
                {
                    matchedPositions.Add(new Vector3Int(itemX, y, 0));
                }
                else
                {
                    break;
                }
            }

            // Check downwards
            for (int y = itemY - 1; y >= 0; y--)
            {
                if (board[itemX, y] != null && board[itemX, y].ItemType == currentItem.ItemType)
                {
                    matchedPositions.Add(new Vector3Int(itemX, y, 0));
                }
                else
                {
                    break;
                }
            }

            return matchedPositions.Count >= 3 ? matchedPositions : new List<Vector3Int>();
        }
        private async UniTask<bool> HasMatch(Vector3Int pos)
        {
            List<Vector3Int> verticalMatches = CheckVerticalMatches(_tileItems, pos.x, pos.y, width, height);
            List<Vector3Int> horizontalMatches = CheckHorizontalMatches(_tileItems, pos.x, pos.y, width, height);

            if (Mathf.Max(verticalMatches.Count, horizontalMatches.Count) < 3)
            {
                lerpingItems.Remove(pos);
                return false;
            }
            if(verticalMatches.Count>horizontalMatches.Count)
            {
                if (verticalMatches.Count == 5)
                {
                    verticalMatches.RemoveAt(0);
                    _tileItems[pos.x, pos.y].OnMatch();
                    _tileItems[pos.x,pos.y]=_objectPool.GetItemFromPool(5, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
                    _tileItems[pos.x, pos.y].Transform.parent = transform;
                    await HandleMatchWithDelay(verticalMatches);

                    return true;
                }else if (verticalMatches.Count == 4)
                {
                    verticalMatches.RemoveAt(0);
                    _tileItems[pos.x, pos.y].OnMatch();
                    //_tileItems[pos.x,pos.y]=_objectPool.GetItemFromPool(7, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
                    _tileItems[pos.x,pos.y]=_objectPool.GetItemFromPool(5, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
                    _tileItems[pos.x, pos.y].Transform.parent = transform;

                    await HandleMatchWithDelay(verticalMatches);

                    return true;
                }
                else if(verticalMatches.Count==3)
                {
                    await HandleMatchWithDelay(verticalMatches);
                    return true;
                }

            }
            
        
            if (horizontalMatches.Count == 5)
            {
                horizontalMatches.RemoveAt(0);
                _tileItems[pos.x, pos.y].OnMatch();
                //_tileItems[pos.x, pos.y] = _objectPool.GetItemFromPool(5, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
                _tileItems[pos.x,pos.y]=_objectPool.GetItemFromPool(5, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
                _tileItems[pos.x, pos.y].Transform.parent = transform;

                await HandleMatchWithDelay(horizontalMatches);

                return true;
            }else if (horizontalMatches.Count == 4)
            {
                horizontalMatches.RemoveAt(0);
                _tileItems[pos.x, pos.y].OnMatch();
               // _tileItems[pos.x,pos.y]=_objectPool.GetItemFromPool(6, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
               _tileItems[pos.x,pos.y]=_objectPool.GetItemFromPool(5, grid.GetCellCenterWorld(pos)).GetComponent<IItem>();
               _tileItems[pos.x, pos.y].Transform.parent = transform;

               await HandleMatchWithDelay(horizontalMatches);
                return true;
            }
            _tileItems[pos.x, pos.y].Transform.localPosition=grid.GetCellCenterLocal(pos);
            
            await  HandleMatchWithDelay(horizontalMatches);
            return true;
            
        }
        
    
        
     

        private bool isBoardDirty;
        private int[] missingItems=new int[8];
        [SerializeField]private int[] y = new int[8];
        
        private async UniTask HandleMatchWithDelay(List<Vector3Int> matchedItems, float delayBetweenMatches = 0.1f,float delayToFillBoard=0.1f)
        {
            foreach (var item in matchedItems)
            {
                if (_tileItems[item.x, item.y] == null)
                {
                    return;
                }
                _tileItems[item.x, item.y].OnMatch();
                _objectPool.GetParticleEffectFromPool(_tileItems[item.x, item.y].ItemType, grid.GetCellCenterWorld(item));
                await UniTask.Delay((int)(delayBetweenMatches * 1000));
            }
            await UniTask.Delay((int)(delayToFillBoard * 1000));
            foreach (var item in matchedItems)
            {
                missingItems[item.x]++;
                _tileItems[item.x, item.y] = null;

            }
        }
        
  
        private async UniTask HandleBombMatch(List<Vector3Int> matchedItems)
        {
            
            foreach (var item in matchedItems)
            {
                _tileItems[item.x, item.y].OnMatch();
                _objectPool.GetParticleEffectFromPool(_tileItems[item.x, item.y].ItemType, grid.GetCellCenterWorld(item));
                _tileItems[item.x, item.y] = null;

            }

            await UniTask.Delay(400);
            foreach (var item in matchedItems)
            {
                missingItems[item.x]++;
            }
            
        }
     

        // Screen shake on board but particle effects dont change their position
        //Board items are also changing their position so making grid their parent might be a good idea
        //first board goes to -1,-1(left,bottom) and then goes to 0,0
        public float shakeDuration = 0.5f;
        public float shakeMagnitude = 0.1f;

        private async UniTaskVoid BoardShake()
        {
            Vector3 originalPosition = transform.position;
            float elapsed = 0.0f;
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;

                transform.position = new Vector3(originalPosition.x+x,originalPosition.y+ y, originalPosition.z);

                elapsed += Time.deltaTime;

                await UniTask.Yield();
            }

            transform.position = originalPosition;
        }

        
     

     
    }
}